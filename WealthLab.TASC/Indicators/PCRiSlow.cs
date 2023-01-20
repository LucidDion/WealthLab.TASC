using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    /// <summary>
    /// PCRiSlow (Slow Put/Call Ratio Indicator)
    /// </summary>
    public class PCRiSlow : IndicatorBase
    {
        //parameterless constructor
        public PCRiSlow() : base()
        {
        }

        //for code based construction
        public PCRiSlow(TimeSeries source, Int32 rsiPeriod, Int32 wmaPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = rsiPeriod;
            Parameters[2].Value = wmaPeriod;

            Populate();
        }

        //static method
        public static PCRiSlow Series(TimeSeries source, int rsiPeriod, int wmaPeriod)
        {
            string key = CacheKey("PCRiSlow", rsiPeriod, wmaPeriod);
            if (source.Cache.ContainsKey(key))
                return (PCRiSlow)source.Cache[key];
            PCRiSlow p = new PCRiSlow(source, rsiPeriod, wmaPeriod);
            source.Cache[key] = p;
            return p;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Raw P/C Ratio", ParameterType.TimeSeries, PriceComponent.Close);   //raw Put/Call Ratio of Equities
            AddParameter("Rainbow Period", ParameterType.Int32, 5);
            AddParameter("WMA Smooth Period", ParameterType.Int32, 1);
        }

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            TimeSeries ds = new TimeSeries(source.DateTimes, false);
            ds.Values.AddRange(source.Values);

            Int32 rainbowPeriod = Parameters[1].AsInt;
            Int32 wmaSmoothingPeriod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            int period = Math.Max(rainbowPeriod, wmaSmoothingPeriod);
            int FirstValidValue = Math.Max(rainbowPeriod * 3, wmaSmoothingPeriod);

            if (period <= 0 || ds.Count == 0 || ds.Count < FirstValidValue)
                return;

            // Clip the  raw Put/Call Ratio data
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (ds[bar] > 0.9) ds[bar] = 0.9;
                else if (ds[bar] < 0.45) ds[bar] = 0.45;
            }

            // Rainbow smoothing			
            var rbw = new TimeSeries(DateTimes);
            var ma = new WMA(ds, rainbowPeriod);

            for (int w = 1; w <= 9; w++)
            {
                ma = new WMA(ma, rainbowPeriod);
                rbw += ma;
            }
            rbw /= 10d;

            for (int bar = wmaSmoothingPeriod - 1; bar < ds.Count; bar++)
            {
                Values[bar] = new WMA(rbw, wmaSmoothingPeriod)[bar];
            }
        }

        public override string Name => "PCRiSlow";

        public override string Abbreviation => "PCRiSlow";

        public override string HelpDescription => "PCRiSlow is the Slow Put/Call Ratio indicator for Equities from the November 2011 issue of TASC Magazine.  Use the CBOE Provider to pass the raw Put/Call Ratio for Equities as the DataSeries input.";

        public override string PaneTag => "PCRiSlow";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}