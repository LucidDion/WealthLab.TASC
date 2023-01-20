using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class MABandLower : IndicatorBase
    {
        //parameterless constructor
        public MABandLower() : base()
        {
        }

        //for code based construction
        public MABandLower(TimeSeries source, Int32 period1, Int32 period2, Double deviation)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = deviation;

            Populate();
        }

        //static method
        public static MABandLower Series(TimeSeries source, int period1, int period2, double deviation)
        {
            string key = CacheKey("MABandLower", period1, period2, deviation);
            if (source.Cache.ContainsKey(key))
                return (MABandLower)source.Cache[key];
            MABandLower mab = new MABandLower(source, period1, period2, deviation);
            source.Cache[key] = mab;
            return mab;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("MA Period 1", ParameterType.Int32, 50);
            AddParameter("MA Period 2", ParameterType.Int32, 10);
            AddParameter("Deviation", ParameterType.Double, 1.0);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Double Mltp = Parameters[3].AsDouble;
            int period = Math.Max(period1, period2);

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0 || ds.Count < period)
                return;

            EMA MA1 = EMA.Series(ds, period1);
            EMA MA2 = EMA.Series(ds, period2);
            TimeSeries Dst = MA1 - MA2;
            TimeSeries DV = (Dst * Dst).Sum(period2) / (double)period2;
            TimeSeries Dev = TimeSeries.Sqrt(DV) * Mltp;
            //TimeSeries UPBND = MA1 + Dev;
            TimeSeries DWNBND = MA1 - Dev;

            Values = DWNBND.Values;

            PrefillNan(Math.Max(period1, period2));
        }

        public override string Name => "MABandLower";

        public override string Abbreviation => "MABandLower";

        public override string HelpDescription => "Lower Moving Average Band by Vitali Apirine from the August 2021 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => "Price";

        public override WLColor DefaultColor => WLColor.Red;

        //bands
        public override PlotStyle DefaultPlotStyle => PlotStyle.Bands;

        public override List<string> Companions => new List<string>() { "MABandUpper" };
    }
}