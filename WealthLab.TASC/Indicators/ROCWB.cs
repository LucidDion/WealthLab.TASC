using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ROCWB : IndicatorBase
    {
        //parameterless constructor
        public ROCWB() : base()
        {
        }

        //for code based construction
        public ROCWB(TimeSeries source, int period, int period2, int period3)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = period2;
            Parameters[3].Value = period3;
            Populate();
        }

        //static method
        public static ROCWB Series(TimeSeries source, int period, int period2, int period3)
        {
            string key = CacheKey("RoofingFilter");
            if (source.Cache.ContainsKey(key))
                return (ROCWB)source.Cache[key];
            ROCWB r = new ROCWB(source, period, period2, period3);
            source.Cache[key] = r;
            return r;
        }

        public override string Name => "Rate Of Change with Bands";

        public override string Abbreviation => "ROCWB";

        public override string HelpDescription => "From Vitali Apirine's article from Stocks and Commodities March 2021 issue.";

        public override string PaneTag => "ROC";

        //don't expose this auxiliary indicator
        public override bool IsPrivate => true;

        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 period3 = Parameters[3].AsInt;

            DateTimes = source.DateTimes;
            if (period <= 0)
                return;

            ROC roc = ROC.Series(source, period);
            var AvrgOfSquares = (roc * roc).Sum(period3) / period3;
            var RocDev = TimeSeries.Sqrt(AvrgOfSquares);
            var MaRateOfChg = EMA.Series(roc, period2);

            Values = RocDev.Values;
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("ROC Period", ParameterType.Int32, 12);
            AddParameter("EMA Period", ParameterType.Int32, 3);
            AddParameter("Average of Squares Period", ParameterType.Int32, 12);
        }
    }
}