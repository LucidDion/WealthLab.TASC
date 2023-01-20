using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class AEMA : IndicatorBase
    {
        public override string Name
        {
            get
            {
                return "Adaptive Exponential Moving Average";
            }
        }
        public override string Abbreviation
        {
            get
            {
                return "AEMA";
            }
        }
        public override string HelpDescription
        {
            get
            {
                return "Adaptive Exponential Moving Average, based on the article by Vitali Apirine in the April 2019 issue of Stocks & Commodities magazine.";
            }
        }
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DarkBlue;
            }
        }

        public AEMA()
        {
        }
        public AEMA(BarHistory source, int period1 = 30, int period2 = 30)
        {
            base.Parameters[0].Value = source;
            base.Parameters[1].Value = period1;
            base.Parameters[2].Value = period2;
            this.Populate();
        }

        //static method
        public static AEMA Series(BarHistory source, int period1, int period2)
        {
            string key = CacheKey("AEMA", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (AEMA)source.Cache[key];
            AEMA aema = new AEMA(source, period1, period2);
            source.Cache[key] = aema;
            return aema;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            base.AddParameter("Time Periods", ParameterType.Int32, 30);
            base.AddParameter("Lookback Period", ParameterType.Int32, 30);
        }
        public override void Populate()
        {
            BarHistory bars = base.Parameters[0].AsBarHistory;
            int period1 = base.Parameters[1].AsInt;
            int period2 = base.Parameters[2].AsInt;
            this.DateTimes = bars.DateTimes;
            int FirstValidValue = Math.Max(period1, period2) + 1;
            if (bars.Count < FirstValidValue)
            {
                return;
            }
            var MLTP = 2.0 / ((double)period1 + 1.0);
            var MLTP2 = (bars.Close - Lowest.Series(bars.Low, period2) - (Highest.Series(bars.High, period2) - bars.Close)).Abs() / (Highest.Series(bars.High, period2) - Lowest.Series(bars.Low, period2));
            var timeSeries = MLTP * (1.0 + MLTP2);

            for (int i = 0; i < FirstValidValue; i++)
            {
                base[i] = FastSMA.Series(bars.Close, period1)[i];
            }

            for (int j = FirstValidValue; j < bars.Count; j++)
            {
                base.Values[j] = base.Values[j - 1] + timeSeries[j] * (bars.Close[j] - base.Values[j - 1]);
            }
        }
    }
}