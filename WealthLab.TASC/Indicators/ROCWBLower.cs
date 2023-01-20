using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ROCWBLower : IndicatorBase
    {
        //parameterless constructor
        public ROCWBLower() : base()
        {
        }

        //for code based construction
        public ROCWBLower(TimeSeries source, int period, int period2, int period3)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = period2;
            Parameters[3].Value = period3;

            Populate();
        }

        //static method
        public static ROCWBLower Series(TimeSeries source, int period, int period2, int period3)
        {
            string key = CacheKey("ROCWBLower", period, period2, period3);
            if (source.Cache.ContainsKey(key))
                return (ROCWBLower)source.Cache[key];
            ROCWBLower rbl = new ROCWBLower(source, period, period2, period3);
            source.Cache[key] = rbl;
            return rbl;
        }

        public override string Name => "ROC with Bands Lower";

        public override string Abbreviation => "ROCWBLower";

        public override string HelpDescription => ("From Vitali Apirine's article from Stocks and Commodities March 2021 issue.");

        public override string PaneTag => "ROC";

        //default color
        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Bands;

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 period3 = Parameters[3].AsInt;

            DateTimes = source.DateTimes;
            if (period < 1 || period2 < 1 || period3 < 1 || DateTimes.Count == 0)
                return;

            ROCWB r = ROCWB.Series(source, period, period2, period3);
            for (int i = 0; i < DateTimes.Count; i++)
                Values[i] = r[i] * -1;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("ROC Period", ParameterType.Int32, 12);
            AddParameter("EMA Period", ParameterType.Int32, 3);
            AddParameter("Average of Squares Period", ParameterType.Int32, 12);
        }

        //companions
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("ROCWBUpper");
                return c;
            }
        }
    }
}