using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ElegantOscillator : IndicatorBase
    {
        //parameterless constructor
        public ElegantOscillator() : base()
        {
        }

        //for code based construction
        public ElegantOscillator(TimeSeries source, Int32 bandEdge)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = bandEdge;

            Populate();
        }

        //static method
        public static ElegantOscillator Series(TimeSeries source, int bandEdge)
        {
            string key = CacheKey("ElegantOscillator", bandEdge);
            if (source.Cache.ContainsKey(key))
                return (ElegantOscillator)source.Cache[key];
            ElegantOscillator eo = new ElegantOscillator(source, bandEdge);
            source.Cache[key] = eo;
            return eo;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Band Edge", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 bandEdge = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (bandEdge <= 0 || ds.Count < 50)
                return;

            var FirstValidValue = Math.Max(50, bandEdge);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            //Integrate with SuperSmoother
            double a1 = Math.Exp(-1.414 * Math.PI / bandEdge);
            double b1 = 1.414 * 180.0 / bandEdge;
            b1 = b1.ToRadians();
            b1 = 2.0 * a1 * Math.Cos(b1);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1.0 - c2 - c3;

            TimeSeries Deriv = ds - (ds >> 2);
            TimeSeries NDeriv = new TimeSeries(ds.DateTimes, 0.0);
            TimeSeries IFish = new TimeSeries(ds.DateTimes, 0.0);
            TimeSeries SS = new TimeSeries(ds.DateTimes, 0.0);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < FirstValidValue || double.IsNaN(SS[bar]))
                    SS[bar] = 0d;
                else
                {
                    //Normalize to standard deviation
                    double RMS = 0;
                    for (int count = 0; count < 49; count++)
                        RMS += Deriv[bar - count] * Deriv[bar - count];

                    if (RMS != 0)
                        RMS = Math.Sqrt(RMS / (double)50);
                    NDeriv[bar] = Deriv[bar] / RMS;

                    //Compute the Inverse Fisher Transform
                    IFish[bar] = (Math.Exp(2 * NDeriv[bar]) - 1.0) / (Math.Exp(2 * NDeriv[bar]) + 1.0);

                    //Integrate with SuperSmoother
                    SS[bar] = c1 * (IFish[bar] + IFish[bar - 1]) / 2.0 + c2 * SS[bar - 1] + c3 * SS[bar - 2];
                }
            }

            Values = SS.Values;

            PrefillNan(FirstValidValue);
        }

        public override string Name => "ElegantOscillator";

        public override string Abbreviation => "ElegantOscillator";

        public override string HelpDescription => "Created by John Ehlers, the Elegant Oscillator is an oscillator that can be used in swing trading. It uses the inverse Fisher transform to help spot reversion-to-the-mean opportunities.";

        public override string PaneTag => @"EO";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}