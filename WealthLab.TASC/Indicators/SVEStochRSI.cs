using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SVEStochRSI : IndicatorBase
    {
        //parameterless constructor
        public SVEStochRSI() : base()
        {
            OverboughtLevel = 80;
            OversoldLevel = 20;
        }

		//for code based construction
		public SVEStochRSI(TimeSeries ds, Int32 rsiPeriod, Int32 stochPeriod, Int32 smaPeriod)
			: base()
		{
			Parameters[0].Value = ds;
			Parameters[1].Value = rsiPeriod;
			Parameters[2].Value = stochPeriod;
			Parameters[2].Value = smaPeriod;
            OverboughtLevel = 80;
            OversoldLevel = 20;
            Populate();
		}

		//static method
		public static SVEStochRSI Series(TimeSeries ds, Int32 rsiPeriod, Int32 stochPeriod, Int32 smaPeriod)
	{
			string key = CacheKey("SVEStochRSI", rsiPeriod, stochPeriod, smaPeriod);
			if (ds.Cache.ContainsKey(key))
				return (SVEStochRSI)ds.Cache[key];
			SVEStochRSI sve = new SVEStochRSI(ds, rsiPeriod, stochPeriod, smaPeriod);
			ds.Cache[key] = sve;
			return sve;
		}

		//generate parameters
		protected override void GenerateParameters()
		{
			AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
			AddParameter("RSI period", ParameterType.Int32, 21);
			AddParameter("Stochastic period", ParameterType.Int32, 5);
			AddParameter("Stochastic SMA Smoothing period", ParameterType.Int32, 8);
		}

		public override void Populate()
		{
			TimeSeries ds = Parameters[0].AsTimeSeries;
			Int32 rsiPeriod = Parameters[1].AsInt;
			Int32 stochPeriod = Parameters[2].AsInt;
			Int32 smaPeriod = Parameters[3].AsInt;

			DateTimes = ds.DateTimes;
			var period = Math.Max(Math.Max(rsiPeriod, stochPeriod), smaPeriod);

			if (period <= 0 || DateTimes.Count == 0)
				return;

			// First we buffer the RSI indicator --------------------------------------------------
			var rsi = new RSI(ds, rsiPeriod);

			// Buffering the Highest High and lowest low RSI during the Stochastic lookback period
			var HiRSI_Buffer = Highest.Series(rsi, stochPeriod);
			var LowRSI_Buffer = Lowest.Series(rsi, stochPeriod);

			// Now we buffer the RSI minus the Low RSI value of the lookback period
			// Doing the same for the High minus Low RSI value of the lookback period.
			var RSILow_Buffer = new TimeSeries(DateTimes);
			var HiLow_Buffer = new TimeSeries(DateTimes);

			for (int i = 0; i < DateTimes.Count; i++)
			{
				RSILow_Buffer[i] = (rsi[i] - LowRSI_Buffer[i]);
				HiLow_Buffer[i] = (HiRSI_Buffer[i] - LowRSI_Buffer[i]);
			}

			// Next action is creating the SMA of this 2 last values			
			var ema_Buffer1 = SMA.Series(RSILow_Buffer, smaPeriod);
			var ema_Buffer2 = SMA.Series(HiLow_Buffer, smaPeriod);

			// Finally the Stochastics formula is applied
			// %K = (Current Close - Lowest Low)/(Highest High - Lowest Low) * 100

			for (int bar = period; bar < ds.Count; bar++)
			{
				Values[bar] = ema_Buffer1[bar] / (0.1 + (ema_Buffer2[bar])) * 100;
			}
		}

        public override string Name => "SVEStochRSI";

        public override string Abbreviation => "SVEStochRSI";

        public override string HelpDescription => "SVEStochRSI by Sylvain Vervoort from February 2019 issue of Technical Analysis of Stocks & Commodities magazine is a tool for confirming price reversals based on divergences between price and indicator.";

        public override string PaneTag => @"SVEStochRSI";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}