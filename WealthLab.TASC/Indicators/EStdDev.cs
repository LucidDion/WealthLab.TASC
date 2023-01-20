using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class EStdDev : IndicatorBase
    {
        //parameterless constructor
        public EStdDev() : base()
        {
        }

        //for code based construction
        public EStdDev(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static EStdDev Series(TimeSeries source, int period)
        {
            string key = CacheKey("EStdDev", period);
            if (source.Cache.ContainsKey(key))
                return (EStdDev)source.Cache[key];
            EStdDev esd = new EStdDev(source, period);
            source.Cache[key] = esd;
            return esd;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Length", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var weight = 2d / (period + 1);
            var esd = new TimeSeries(DateTimes);
            var ema = new TimeSeries(DateTimes);

            esd[period - 1] = 0d;
            ema[period - 1] = 0d;

            //Based on formula by Alex Matulich
            //http://unicorn.us.com/trading/src/_xStdDev.txt
            //http://unicorn.us.com/trading/xstdev2.xls

            for (int bar = period; bar < ds.Count; bar++)
            {
                ema[bar] = weight * ds[bar] + (1 - weight) * ema[bar - 1];
                esd[bar] = Math.Sqrt(weight * (ds[bar] - ema[bar]) * (ds[bar] - ema[bar]) + (1d - weight) * esd[bar - 1] * esd[bar - 1]);
                Values[bar] = esd[bar];
            }
        }

        public override string Name => "EStdDev";

        public override string Abbreviation => "EStdDev";

        public override string HelpDescription => "Exponential standard deviation from the February 2017 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "ESDBandsPane";

        public override WLColor DefaultColor => WLColor.Navy;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }
}