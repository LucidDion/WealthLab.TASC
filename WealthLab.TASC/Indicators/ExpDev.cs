using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ExpDev : IndicatorBase
    {
        //parameterless constructor
        public ExpDev() : base()
        {
        }

        //for code based construction
        public ExpDev(TimeSeries source, Int32 period, bool useEMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = useEMA;

            Populate();
        }

        //static method
        public static ExpDev Series(TimeSeries source, int period, bool useEMA)
        {
            string key = CacheKey("ExpDev", period, useEMA);
            if (source.Cache.ContainsKey(key))
                return (ExpDev)source.Cache[key];
            ExpDev ed = new ExpDev(source, period, useEMA);
            source.Cache[key] = ed;
            return ed;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 20);
            AddParameter("Use EMA", ParameterType.Boolean, false);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Boolean useEMA = Parameters[2].AsBoolean;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var weight = 2d / (period + 1);
            TimeSeries sma = FastSMA.Series(ds, period);
            TimeSeries ema = EMA.Series(ds, period);
            TimeSeries ma = !useEMA ? sma : ema;

            double mdev = 0;
            var exd = new TimeSeries(DateTimes);
            var absDif = (ma - ds).Abs();
            var Rate = 2 / (double)(period + 1);

            for (int i = 0; i > period; i--)
            {
                mdev += Math.Abs(ma[i] - ds[i]);
            }

            mdev /= period;

            for (int bar = 1; bar < ds.Count; bar++)
            {
                if (bar <= period)
                    exd[bar] = mdev;
                else
                    exd[bar] = absDif[bar] * Rate + exd[bar - 1] * (1.0 - Rate);
            }

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = exd[bar];
            }

            PrefillNan(period + 1);

        }

        public override string Name => "ExpDev";

        public override string Abbreviation => "ExpDev";

        public override string HelpDescription => "Exponential deviation from the July 2019 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "ExpDevBandsPane";

        public override WLColor DefaultColor => WLColor.Navy;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }
}