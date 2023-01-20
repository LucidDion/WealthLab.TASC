using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class BandPass : IndicatorBase
    {
        //parameterless constructor
        public BandPass() : base()
        {
        }

        //for code based construction
        public BandPass(TimeSeries source, Int32 period, double bandwidth)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = bandwidth;
            Populate();
        }

        //static method
        public static BandPass Series(TimeSeries source, int period, double bandwidth)
        {
            string key = CacheKey("BandPass", period, bandwidth);
            if (source.Cache.ContainsKey(key))
                return (BandPass)source.Cache[key];
            BandPass bp = new BandPass(source, period, bandwidth);
            source.Cache[key] = bp;
            return bp;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 20);
            AddParameter("Bandwidth", ParameterType.Double, 0.1);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            double bandWidth = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(3, period);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            double Deg2Rad = Math.PI / 180.0;
            var F1 = Math.Cos((360d / (double)period) * Deg2Rad);
            var G1 = Math.Cos((bandWidth * 360 / (double)period) * Deg2Rad);
            var S1 = 1d / G1 - Math.Sqrt(1d / (G1 * G1) - 1);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = 0;
            }

            //BandPass Filter
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                if (bar <= 5)
                    Values[bar] = 0;

                if (bar > period)
                    Values[bar] = .5 * (1 - S1) * (ds[bar] - ds[bar - 2]) + F1 * (1 + S1) * Values[bar - 1] - S1 * Values[bar - 2];
            }

            PrefillNan(period + 1);
        }

        public override string Name => "Bandpass";

        public override string Abbreviation => "BandPass";

        public override string HelpDescription => "Two-pole bandpass filter created by John Ehlers.";

        public override string PaneTag => @"Bandpass";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}