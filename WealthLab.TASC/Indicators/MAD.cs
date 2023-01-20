using WealthLab.Core;

namespace WealthLab.Indicators
{
    public class MAD : IndicatorBase
    {
        //parameterless constructor
        public MAD() : base()
        {
        }

        //for code based construction
        public MAD(TimeSeries ds, Int32 period1, Int32 period2)
            : base()
        {
			Parameters[0].Value = ds;
			Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static MAD Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("MAD", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (MAD)source.Cache[key];
            MAD zs = new MAD(source, period1, period2);
            source.Cache[key] = zs;
            return zs;
        }

        public override string Name => "MAD";

		public override string Abbreviation => "MAD";

		public override string HelpDescription => @"MAD (Moving Average Difference) indicator by John F. Ehlers from the October 2021 issue of Technical Analysis of Stocks & Commodities magazine.";

		public override string PaneTag => @"MAD";

		public override WLColor DefaultColor => WLColor.DarkRed;

		public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

        //populate
        public override void Populate()
        {
			TimeSeries ds = Parameters[0].AsTimeSeries;
			Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

			if (period1 <= 0 || period2 <= 0 || ds.Count == 0)
				return;

            //MAD = 100 * (Average(Close, ShortLength) - Average(Close, LongLength)) / Average(Close, LongLength);
            var sma1 = FastSMA.Series(ds, period1);
            var sma2 = FastSMA.Series(ds, period2);
            var mad = 100 * (sma1 - sma2) / sma2;

			for (int bar = 0; bar < ds.Count; bar++)
			{
				Values[bar] = mad[bar];
			}
            PrefillNan(Math.Max(period1, period2));
        }

        //generate parameters
        protected override void GenerateParameters()
        {
			AddParameter("Data Series", ParameterType.TimeSeries, PriceComponent.Close);
			AddParameter("Fast SMA period", ParameterType.Int32, 8);
            AddParameter("Slow SMA period", ParameterType.Int32, 23);
        }
    }
}