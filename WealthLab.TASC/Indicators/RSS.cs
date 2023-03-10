using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //RSS Indicator class
    public class RSS : IndicatorBase
    {
        //parameterless constructor
        public RSS() : base()
        {
        }

        //for code based construction
        public RSS(TimeSeries source, int fastSMAPeriod, int slowSMAPeriod, int rsiPeriod, int smoothPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastSMAPeriod;
            Parameters[2].Value = slowSMAPeriod;
            Parameters[3].Value = rsiPeriod;
            Parameters[4].Value = smoothPeriod;

            Populate();
        }

        //static method
        public static RSS Series(TimeSeries source, int fastSMAPeriod, int slowSMAPeriod, int rsiPeriod, int smoothPeriod)
        {
            string key = CacheKey("RSS", fastSMAPeriod, slowSMAPeriod, rsiPeriod, smoothPeriod);
            if (source.Cache.ContainsKey(key))
                return (RSS)source.Cache[key];
            RSS rss = new RSS(source, fastSMAPeriod, slowSMAPeriod, rsiPeriod, smoothPeriod);
            source.Cache[key] = rss;
            return rss;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Fast SMA Period", ParameterType.Int32, 10);
            AddParameter("Slow SMA Period", ParameterType.Int32, 40);
            AddParameter("RSI Period", ParameterType.Int32, 5);
            AddParameter("Smoothing Period", ParameterType.Int32, 5);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 fastsmaperiod = Parameters[1].AsInt;
            Int32 slowsmaperiod = Parameters[2].AsInt;
            Int32 rsiperiod = Parameters[3].AsInt;
            Int32 smoothperiod = Parameters[4].AsInt;

            int period = new List<int> { fastsmaperiod, slowsmaperiod, rsiperiod, smoothperiod }.Max();

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Remember parameters 
            var spread = SMA.Series(ds, fastsmaperiod) - SMA.Series(ds, slowsmaperiod);
            var relstr = RSI.Series(spread, rsiperiod);
            var smooth = SMA.Series(relstr, smoothperiod);

            //Assign first bar that contains indicator data
            var FirstValidValue = (fastsmaperiod > slowsmaperiod ? fastsmaperiod : slowsmaperiod) + rsiperiod + smoothperiod;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < FirstValidValue)
                    Values[bar] = 0;
                else
                    Values[bar] = smooth[bar];
            }
            PrefillNan(period + smoothperiod + rsiperiod);
        }

        public override string Name => "RSS";

        public override string Abbreviation => "RSS";

        public override string HelpDescription => "Relative Spread Strength indicator, from Ian Copsey's article in the October 2006 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"RSS";

        public override WLColor DefaultColor => WLColor.BlueViolet;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}