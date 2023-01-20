using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //BBandPercentB Indicator class
    public class BollingerPctB : IndicatorBase
    {
        //parameterless constructor
        public BollingerPctB() : base()
        {
        }

        //for code based construction
        public BollingerPctB(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static BollingerPctB Series(TimeSeries source, int period)
        {
            string key = CacheKey("BollingerPctB", period);
            if (source.Cache.ContainsKey(key))
                return (BollingerPctB)source.Cache[key];
            BollingerPctB bPctB = new BollingerPctB(source, period);
            source.Cache[key] = bPctB;
            return bPctB;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var _sd = StdDev.Series(ds, period);
            var _sma = FastSMA.Series(ds, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = 100 * (ds[bar] + 2 * _sd[bar] - _sma[bar]) / (4 * _sd[bar]);
            }
        }



        public override string Name => "BBandPercentB";

        public override string Abbreviation => "BBandPercentB";

        public override string HelpDescription => @"Bollinger %b referenced in the May 2010 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"BBandPercentB";

        public override WLColor DefaultColor => WLColor.Green;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    } 

}