using WealthLab.Core;

namespace WealthLab.Indicators
{
    public class MADH : IndicatorBase
    {
        TimeSeries FIR_Hann(TimeSeries source, int period)
        {
            TimeSeries ds = new TimeSeries(source.DateTimes, 0);
            TimeSeries Filt = new TimeSeries(source.DateTimes, 0);

            double coef = 1.0 - Math.Cos(((360 * (double)period) / (period + 1)).ToRadians());

            for (int bar = 0; bar < source.Count; bar++)
            {
                if (bar > period)
                {
                    for (int count = 1; count <= period; count++)
                    {
                        double ang = 360 * count / (period + 1);
                        double c = (1 - Math.Cos(ang.ToRadians()));
                        Filt[bar] += (c * source[bar - count - 1]);
                        coef += c;
                    }
                }

                if (coef != 0)
                    Filt[bar] /= coef;

                ds.Values[bar] = Filt[bar];
            }

            return ds;
        }

        //parameterless constructor
        public MADH() : base()
        {
        }

        //for code based construction
        public MADH(TimeSeries ds, Int32 shortLength, Int32 dominantCycle)
            : base()
        {
			Parameters[0].Value = ds;
			Parameters[1].Value = shortLength;
            Parameters[2].Value = dominantCycle;

            Populate();
        }

        //static method
        public static MADH Series(TimeSeries source, int shortLength, int dominantCycle)
        {
            string key = CacheKey("MADH", shortLength, dominantCycle);
            if (source.Cache.ContainsKey(key))
                return (MADH)source.Cache[key];
            MADH zs = new MADH(source, shortLength, dominantCycle);
            source.Cache[key] = zs;
            return zs;
        }

        public override string Name => "MADH";

		public override string Abbreviation => "MADH";

        public override string HelpDescription => @"MADH (Moving Average Difference, Enhanced) indicator by John F. Ehlers from the November 2021 issue of Technical Analysis of Stocks & Commodities magazine.";

		public override string PaneTag => @"MADH";

		public override WLColor DefaultColor => WLColor.Yellow;

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

            TimeSeries filt1 = FIR_Hann(ds, period1);
            int LongLength = Convert.ToInt16(period1 + period2/ 2);
            TimeSeries filt2 = FIR_Hann(ds, LongLength);
            TimeSeries madh = 100 * (filt1 - filt2) / filt2;

			for (int bar = 0; bar < ds.Count; bar++)
			{
				Values[bar] = madh[bar];
			}
            PrefillNan(Math.Max(period1, period2));
        }

        //generate parameters
        protected override void GenerateParameters()
        {
			AddParameter("Data Series", ParameterType.TimeSeries, PriceComponent.Close);
			AddParameter("Short length", ParameterType.Int32, 8);
            AddParameter("Dominant cycle", ParameterType.Int32, 27);
        }
    }
}