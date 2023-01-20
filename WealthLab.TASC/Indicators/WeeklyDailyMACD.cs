using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class WeeklyMACD : IndicatorBase
    {
        //parameterless constructor
        public WeeklyMACD() : base()
        {
        }

        //for code based construction
        public WeeklyMACD(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static WeeklyMACD Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("WeeklyMACD", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (WeeklyMACD)source.Cache[key];
            WeeklyMACD wm = new WeeklyMACD(source, period1, period2);
            source.Cache[key] = wm;
            return wm;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Weekly Length 1", ParameterType.Int32, 60);
            AddParameter("Weekly Length 2", ParameterType.Int32, 130);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 WeeklyLength1 = Parameters[1].AsInt;
            Int32 WeeklyLength2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            int period = Math.Max(WeeklyLength1, WeeklyLength2);
            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(WeeklyLength1, WeeklyLength2);
            var WM = new EMA(ds, WeeklyLength1) - new EMA(ds, WeeklyLength2);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = WM[bar];
            }
        }

        public override string Name => "WeeklyMACD";

        public override string Abbreviation => "WeeklyMACD";

        public override string HelpDescription => "Weekly MACD by Vitali Apirine from the December 2017 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"WDMACDPane";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}