using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class UniversalOscillator : IndicatorBase
    {
        //parameterless constructor
        public UniversalOscillator() : base()
        {
        }

        //for code based construction
        public UniversalOscillator(TimeSeries source, Int32 bandEdge)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = bandEdge;

            Populate();
        }

        //static method
        public static UniversalOscillator Series(TimeSeries source, int bandEdge)
        {
            string key = CacheKey("UniversalOscillator", bandEdge);
            if (source.Cache.ContainsKey(key))
                return (UniversalOscillator)source.Cache[key];
            UniversalOscillator uo = new UniversalOscillator(source, bandEdge);
            source.Cache[key] = uo;
            return uo;
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

            if (ds.Count == 0)
                return;

            var FirstValidValue = 2;
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;
            if (ds.Count == 0) FirstValidValue = 2;

            double Deg2Rad = Math.PI / 180.0;
            double a1 = Math.Exp(-1.414 * Math.PI / (double)bandEdge);
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / (double)bandEdge) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            var WhiteNoise = new TimeSeries(DateTimes);
            WhiteNoise = ds.Count > 0 ? (ds - (ds >> 2)) / 2d : ds;
            var PeakAGC = new TimeSeries(DateTimes);
            var Filt = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= FirstValidValue)
                {
                    if (bar == 0)
                    {
                        Filt[bar] = 0;
                        PeakAGC[bar] = .0000001;
                    }
                    else
                                        if (bar == 1)
                        Filt[bar] = c1 * 0 * (ds[bar] + ds[bar - 1]) / 2 + c2 * Filt[bar - 1];
                    else
                                            if (bar == 2)
                        Filt[bar] = c1 * 0 * (ds[bar] + ds[bar - 1]) / 2 + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];
                    else
                        Filt[bar] = c1 * (WhiteNoise[bar] + WhiteNoise[bar - 1]) / 2d + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];

                    // Automatic Gain Control (AGC)
                    PeakAGC[bar] = bar > 0 ? 0.991 * PeakAGC[bar - 1] : .0000001;

                    if (Math.Abs(Filt[bar]) > PeakAGC[bar])
                        PeakAGC[bar] = Math.Abs(Filt[bar]);
                    if (PeakAGC[bar] != 0)
                        Values[bar] = Filt[bar] / PeakAGC[bar];
                }
                else
                {
                    PeakAGC[bar] = Filt[bar] = 0d;
                }
            }
            PrefillNan(bandEdge);
        }

        public override string Name => "UniversalOscillator";

        public override string Abbreviation => "UniversalOscillator";

        public override string HelpDescription => @"Created by John Ehlers (see article in January 2015 issue of Stocks and Commodities Magazine), the UniversalOscillator filter is a zero lag indicator that can be used as a basis for creating a swing-trading or trend-trading algorithm.";

        public override string PaneTag => @"UniversalOscillator";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}