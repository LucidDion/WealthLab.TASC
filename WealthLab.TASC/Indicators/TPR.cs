using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class TPR : IndicatorBase
    {
        public override string Name
        {
            get
            {
                return "Trend Persistence Rate";
            }
        }
        public override string Abbreviation
        {
            get
            {
                return "TPR";
            }
        }
        public override string HelpDescription
        {
            get
            {
                return "Trend Persistence Rate, based on the article by Richard Poster in the February 2021 issue of Stocks & Commodities magazine.";
            }
        }
        public override string PaneTag
        {
            get
            {
                return "TPR";
            }
        }
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DarkRed;
            }
        }

        public TPR()
        {
        }
        public TPR(TimeSeries source, int period, int maPeriod, double threshold = 1.0, double mult = 1.0, bool useAbsolute = false)
        {
            base.Parameters[0].Value = source;
            base.Parameters[1].Value = period;
            base.Parameters[2].Value = maPeriod;
            base.Parameters[3].Value = threshold;
            base.Parameters[4].Value = mult;
            base.Parameters[5].Value = useAbsolute;
            this.Populate();
        }

        //static method
        public static TPR Series(TimeSeries source, int period, int maPeriod, double threshold = 1.0, double mult = 1.0, bool useAbsolute = false)
        {
            string key = CacheKey("TPR", period, maPeriod, threshold, mult, useAbsolute);
            if (source.Cache.ContainsKey(key))
                return (TPR)source.Cache[key];
            TPR tpr = new TPR(source, period, maPeriod, threshold, mult, useAbsolute);
            source.Cache[key] = tpr;
            return tpr;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            base.AddParameter("Lookback Period", ParameterType.Int32, 20);
            base.AddParameter("SMA Period", ParameterType.Int32, 5);
            base.AddParameter("Threshold", ParameterType.Double, 1.0);
            base.AddParameter("Multiple", ParameterType.Double, 1.0);
            base.AddParameter("Use absolute numbers", ParameterType.Boolean, true);
        }
        public override void Populate()
        {
            TimeSeries ds = base.Parameters[0].AsTimeSeries;
            int period = base.Parameters[1].AsInt;
            int maPeriod = base.Parameters[2].AsInt;
            double threshold = base.Parameters[3].AsDouble;
            double mult = base.Parameters[4].AsDouble;
            bool useAbs = base.Parameters[5].AsBoolean;

            this.DateTimes = ds.DateTimes;
            int FirstValidValue = period + maPeriod + 1;
            if (ds.Count < FirstValidValue)
            {
                return;
            }
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;
            for (int i = 0; i < FirstValidValue; i++)
            {
                base[i] = 0;
            }

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double tpr, sma1 = 0, sma2 = 0, smadiff = 0;
                int ctrP = 0, ctrM = 0;
                for (int jj = 0; jj < period; jj++)
                {
                    sma1 = FastSMA.Value(bar - jj, ds, maPeriod);
                    sma2 = FastSMA.Value(bar - jj - 1, ds, maPeriod);
                    smadiff = (sma1 - sma2) / mult;
                    if (smadiff > threshold) ctrP += 1; // up trend counter
                    if (smadiff < -threshold) ctrM += 1; // down trend counter
                }
                tpr = 100.0 * (ctrP - ctrM) / threshold;
                base.Values[bar] = !useAbs ? tpr : Math.Abs(tpr);
            }
            PrefillNan(period + maPeriod);
        }
    }
}