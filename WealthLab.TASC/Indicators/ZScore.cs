using WealthLab.Core;

namespace WealthLab.Indicators
{
    public class ZScore : IndicatorBase
    {
        //parameterless constructor
        public ZScore() : base()
        {
        }

        //for code based construction
        public ZScore(TimeSeries ds, Int32 period)
            : base()
        {
			Parameters[0].Value = ds;
			Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static ZScore Series(TimeSeries source, int period)
        {
            string key = CacheKey("ZScore", period);
            if (source.Cache.ContainsKey(key))
                return (ZScore)source.Cache[key];
            ZScore zs = new ZScore(source, period);
            source.Cache[key] = zs;
            return zs;
        }

        public override string Name => "ZScore";

		public override string Abbreviation => "ZScore";

		public override string HelpDescription => @"Z-Score from the May 2019 issue of Technical Analysis of Stocks & Commodities magazine.";

		public override string PaneTag => @"ZScore";

		public override WLColor DefaultColor => WLColor.DarkViolet;

		public override PlotStyle DefaultPlotStyle => PlotStyle.ThickHistogram;

        //populate
        public override void Populate()
        {
			TimeSeries ds = Parameters[0].AsTimeSeries;
			Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

			if (period <= 0 || ds.Count == 0)
				return;
	        
			var sma = SMA.Series(ds, period);
			StdDev sd = StdDev.Series(ds, period);
			TimeSeries zScore = (ds - sma) / sd;

			for (int bar = 0; bar < ds.Count; bar++)
			{
				Values[bar] = zScore[bar];
			}
        }

        //generate parameters
        protected override void GenerateParameters()
        {
			AddParameter("Data Series", ParameterType.TimeSeries, PriceComponent.Close);
			AddParameter("Lookback period", ParameterType.Int32, 10);
        }
    }
}