using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    /// <summary>
    /// PCRiFast (Fast Put/Call Ratio Indicator)
    /// </summary>
    public class PCRiFast : IndicatorBase
    {
        //parameterless constructor
        public PCRiFast() : base()
        {
        }

        //for code based construction
        public PCRiFast(TimeSeries source, Int32 rsiPeriod, Int32 wmaPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = rsiPeriod;
            Parameters[2].Value = wmaPeriod;

            Populate();
        }

        //static method
        public static PCRiFast Series(TimeSeries source, int rsiPeriod, int wmaPeriod)
        {
            string key = CacheKey("PCRiFast", rsiPeriod, wmaPeriod);
            if (source.Cache.ContainsKey(key))
                return (PCRiFast)source.Cache[key];
            PCRiFast p = new PCRiFast(source, rsiPeriod, wmaPeriod);
            source.Cache[key] = p;
            return p;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Raw P/C Ratio", ParameterType.TimeSeries, PriceComponent.Close);   //raw Put/Call Ratio of Equities
            AddParameter("RSI Period", ParameterType.Int32, 5);
            AddParameter("WMA Period", ParameterType.Int32, 5);
        }

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            TimeSeries ds = new TimeSeries(source.DateTimes, false);
            ds.Values.AddRange(source.Values);

            Int32 rsiPeriod = Parameters[1].AsInt;
            Int32 wmaPeriod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            int period = Math.Max(rsiPeriod, wmaPeriod);
            int FirstValidValue = Math.Max(rsiPeriod * 3, wmaPeriod);

            if (period <= 0 || ds.Count == 0 || ds.Count < FirstValidValue)
                return;

            // Clip the raw Put/Call Ratio data
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (ds[bar] > 0.9) ds[bar] = 0.9;
                else if (ds[bar] < 0.45) ds[bar] = 0.45;
            }
            // TEMA smoothing
            ds = new TEMA_TASC(ds, 5);

            // Rainbow smoothing			
            var rbw = new TimeSeries(DateTimes);
            var ma = new WMA(ds, 2);

            for (int w = 1; w <= 9; w++)
            {
                ma = new WMA(ma, 2);
                rbw += ma;
            }
            rbw /= 10d;

            rbw = new RSI(rbw, rsiPeriod);
            for (int bar = wmaPeriod - 1; bar < ds.Count; bar++)
            {
                Values[bar] = new WMA(rbw, wmaPeriod)[bar];
            }
        }

        public override string Name => "PCRiFast";

        public override string Abbreviation => "PCRiFast";

        public override string HelpDescription => "PCRiFast is the Fast Put/Call Ratio indicator for Equities from the November 2011 issue of TASC Magazine.  Use the CBOE Provider to pass the raw Put/Call Ratio for Equities as the DataSeries input.";

        public override string PaneTag => "PCRiFast";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}