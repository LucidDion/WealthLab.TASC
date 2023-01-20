using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ExpDevBandLower : IndicatorBase
    {
        //parameterless constructor
        public ExpDevBandLower() : base()
        {
        }

        //for code based construction
        public ExpDevBandLower(TimeSeries source, Int32 period, Double deviations, Boolean useEMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;
			Parameters[3].Value = useEMA;

            Populate();
        }

        //static method
        public static ExpDevBandLower Series(TimeSeries source, int period, double deviations, bool useEMA)
        {
            string key = CacheKey("ExpDevBandLower", period, deviations, useEMA);
            if (source.Cache.ContainsKey(key))
                return (ExpDevBandLower)source.Cache[key];
            ExpDevBandLower edbl = new ExpDevBandLower(source, period, deviations, useEMA);
            source.Cache[key] = edbl;
            return edbl;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 20);
            AddParameter("Deviations", ParameterType.Double, 2.0);
			AddParameter("Use EMA", ParameterType.Boolean, false);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double deviations = Parameters[2].AsDouble;
			Boolean useEMA = Parameters[3].AsBoolean;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

			TimeSeries sma = FastSMA.Series(ds, period);
            TimeSeries ema = EMA.Series(ds, period);
            TimeSeries ma = !useEMA ? sma : ema;
            ExpDev exd = new ExpDev(ds, period, useEMA);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = ma[bar] - exd[bar] * deviations;
            }
        }

        public override string Name => "ExpDevBandLower";

        public override string Abbreviation => "ExpDevBandLower";

        public override string HelpDescription => "Lower Band of Exponential Deviation Bands from the July 2019 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override WLColor DefaultColor => WLColor.Navy;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Bands;

        public override List<string> Companions => new List<string>() { "ExpDevBandUpper" };
    }
}