using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class CAMA : IndicatorBase
    {
        public override string Name=>"Cong Adaptive Moving Average";
        public override string Abbreviation => "CAMA";
        public override string HelpDescription=>"Cong Adaptive Moving Average, based on the article by Scott Cong in the March 2023 issue of Stocks & Commodities magazine.";
        public override string PaneTag=>"Price";
        public override WLColor DefaultColor => WLColor.Crimson;
        public override bool IsSmoother => false;
        public CAMA()
        {
        }
        public CAMA(BarHistory source, int period = 20)
        {
            base.Parameters[0].Value = source;
            base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static CAMA Series(BarHistory source, int period)
        {
            string key = CacheKey("CAMA", period);
            if (source.Cache.ContainsKey(key))
                return (CAMA)source.Cache[key];
            CAMA cama = new CAMA(source, period);
            source.Cache[key] = cama;
            return cama;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            base.AddParameter("Lookback Period", ParameterType.Int32, 20);
        }
        public override void Populate()
        {
            BarHistory bars = base.Parameters[0].AsBarHistory;
            int period = base.Parameters[1].AsInt;
            this.DateTimes = bars.DateTimes;
            int FirstValidValue = period;
            if (bars.Count < FirstValidValue)
                return;

            TimeSeries Result = Highest.Series(bars.High, period) - Lowest.Series(bars.Low, period);
            TimeSeries Effort = TR.Series(bars).Sum(period);
            TimeSeries alpha = Result / Effort;
            TimeSeries source_price = bars.Close;
            TimeSeries cama = new TimeSeries(DateTimes, 0);
            for (int i = period; i < DateTimes.Count; i++)
            {
                cama[i] = alpha[i] * source_price[i] + (1 - alpha[i]) * cama[i - 1];
            }
            Values = cama.Values;
        }
    }
}