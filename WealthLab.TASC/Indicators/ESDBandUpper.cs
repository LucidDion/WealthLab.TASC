using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ESDBandUpper : IndicatorBase
    {
        //parameterless constructor
        public ESDBandUpper() : base()
        {
        }

        //for code based construction
        public ESDBandUpper(TimeSeries source, Int32 period, Double deviations)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = deviations;

            Populate();
        }

        //static method
        public static ESDBandUpper Series(TimeSeries source, int period, double deviations)
        {
            string key = CacheKey("ESDBandUpper", period, deviations);
            if (source.Cache.ContainsKey(key))
                return (ESDBandUpper)source.Cache[key];
            ESDBandUpper esdbu = new ESDBandUpper(source, period, deviations);
            source.Cache[key] = esdbu;
            return esdbu;
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
                Values[bar] = ema[bar] + esd[bar] * deviations;
            }
        }

        public override string Name => "ESDBandUpper";

        public override string Abbreviation => "ESDBandUpper";

        public override string HelpDescription => "Upper Band of Exponential Standard Deviation Bands from the February 2017 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override WLColor DefaultColor => WLColor.Navy;

        //bands
        public override PlotStyle DefaultPlotStyle => PlotStyle.Bands;

        public override List<string> Companions => new List<string>() { "ESDBandLower" };
    }
}