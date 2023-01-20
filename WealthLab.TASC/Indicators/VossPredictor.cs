using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class VossPredictor : IndicatorBase
    {
        //parameterless constructor
        public VossPredictor() : base()
        {
        }

        //for code based construction
        public VossPredictor(TimeSeries source, Int32 period, Int32 predict)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = predict;

            Populate();
        }

        //static method
        public static VossPredictor Series(TimeSeries source, int period, int predict)
        {
            string key = CacheKey("VossPredictor", period, predict);
            if (source.Cache.ContainsKey(key))
                return (VossPredictor)source.Cache[key];
            VossPredictor vp = new VossPredictor(source, period, predict);
            source.Cache[key] = vp;
            return vp;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 20);
            AddParameter("Predict", ParameterType.Int32, 3);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 predict = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            int maxPeriod = Math.Max(period, predict);

            if (period <= 0 || predict <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(3, maxPeriod);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            var Filt = new BandPass(ds, period, 0.25);
            var voss = new TimeSeries(DateTimes);
            var order = 3 * predict;
            double SumC = 0;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                SumC = 0;

                if (bar <= FirstValidValue)
                    voss[bar] = 0;
                else
                {
                    for (int count = 0; count < order; count++)
                    {
                        SumC += ((count + 1) / (double)order) * voss[bar - (order - count)];
                    }

                    voss[bar] = ((3 + order) / 2) * Filt[bar] - SumC;
                }

                Values[bar] = voss[bar];
            }
            PrefillNan(FirstValidValue + 1);
        }

        public override string Name => "VossPredictor";

        public override string Abbreviation => "VossPredictor";

        public override string HelpDescription => "Created by John Ehlers, the Voss Predictor filter could help signal cyclic turning points.";

        public override string PaneTag => @"Bandpass";

        public override WLColor DefaultColor => WLColor.BlueViolet;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}