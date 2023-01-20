using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ESDBandLower : IndicatorBase
    {
        //parameterless constructor
        public ESDBandLower() : base()
        {
        }

        //for code based construction
        public ESDBandLower(TimeSeries source, Int32 period, Double deviations)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;

            Populate();
        }

        //static method
        public static ESDBandLower Series(TimeSeries source, int period, double deviations)
        {
            string key = CacheKey("ESDBandLower", period, deviations);
            if (source.Cache.ContainsKey(key))
                return (ESDBandLower)source.Cache[key];
            ESDBandLower esdbl = new ESDBandLower(source, period, deviations);
            source.Cache[key] = esdbl;
            return esdbl;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Length", ParameterType.Int32, 20);
            AddParameter("Deviations", ParameterType.Double, 2.0);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double deviations = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            var ema = new EMA(ds, 20);
            var esd = new EStdDev(ds, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = ema[bar] - esd[bar] * deviations;
            }
        }

        public override string Name => "ESDBandLower";

        public override string Abbreviation => "ESDBandLower";

        public override string HelpDescription => "Lower Band of Exponential Standard Deviation Bands from the February 2017 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override WLColor DefaultColor => WLColor.Navy;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Bands;

        public override List<string> Companions => new List<string>() { "ESDBandUpper" };
    }
}