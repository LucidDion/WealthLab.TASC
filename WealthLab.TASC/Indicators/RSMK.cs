using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RSMK : IndicatorBase
    {
        public override string Name
        {
            get
            {
                return "RSMK";
            }
        }
        public override string Abbreviation
        {
            get
            {
                return "RSMK";
            }
        }
        public override string HelpDescription
        {
            get
            {
                return "RSMK is a relative strength indicator based on the article by Markos Katsanos in the March 2020 issue of Stocks & Commodities magazine.";
            }
        }
        public override string PaneTag
        {
            get
            {
                return "RSMK";
            }
        }
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DarkGreen;
            }
        }

        //it's not a smoother
        public override bool IsSmoother => false;

        public RSMK()
        {
        }
        public RSMK(BarHistory bh, string symbol = "SPY", int period = 90, int periodEMA = 3)
        {
            base.Parameters[0].Value = bh;
            base.Parameters[1].Value = symbol;
            base.Parameters[2].Value = period;
            base.Parameters[3].Value = periodEMA;
            this.Populate();
        }

        //static method
        public static RSMK Series(BarHistory source, string symbol = "SPY", int period = 90, int periodEMA = 3)
        {
            string key = CacheKey("RSMK", period);
            if (source.Cache.ContainsKey(key))
                return (RSMK)source.Cache[key];
            RSMK r = new RSMK(source, symbol, period, periodEMA);
            source.Cache[key] = r;
            return r;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            base.AddParameter("Index Symbol", ParameterType.String, "SPY");
            base.AddParameter("Lookback Period", ParameterType.Int32, 90);
            base.AddParameter("EMA Period", ParameterType.Int32, 3);
        }
        public override void Populate()
        {
            BarHistory bars = base.Parameters[0].AsBarHistory;
            string symbol = Parameters[1].AsString;
            BarHistory idx = IndicatorFactory.Instance.GetHistory(bars, symbol, bars.Scale);
            TimeSeries index = idx.Close;
            int period = base.Parameters[2].AsInt;
            int emaPeriod = base.Parameters[3].AsInt;
            this.DateTimes = bars.DateTimes;
            int FirstValidValue = Math.Max(period, emaPeriod) + 1;
            if (bars.Count < FirstValidValue)
            {
                return;
            }

            TimeSeries log1 = new TimeSeries(bars.DateTimes);
            TimeSeries log2 = new TimeSeries(index.DateTimes);
            TimeSeries tmp1 = new TimeSeries(bars.DateTimes);
            tmp1 = bars.Close * 0;

            for (int i = 0; i < FirstValidValue; i++)
            {
                log1[i] = 0;// checked(Math.Log( ds[i] / index[i] ));
                log2[i] = 0;
            }

            TimeSeries ds = bars.Close;
            for (int i = FirstValidValue; i < ds.Count; i++)
            {
                log1[i] = checked(Math.Log(ds[i] / index[i]));
                log2[i] = checked(Math.Log((ds[i - period]) / index[i - period]));
                tmp1[i] = log1[i] - log2[i];
            }

            TimeSeries rsmk = EMA.Series(tmp1, emaPeriod) * 100d;

            for (int j = FirstValidValue; j < ds.Count; j++)
            {
                double val = rsmk[j];
                base.Values[j] = double.IsNaN(val) ? 0 : val;
            }
        }
    }
}