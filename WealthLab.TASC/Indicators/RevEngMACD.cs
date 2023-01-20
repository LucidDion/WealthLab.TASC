using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC 
{
    public class RevEngMACD : IndicatorBase
    {
        private double PMACDeq(int bar, TimeSeries price, int periodX, int periodY, TimeSeries ema1, TimeSeries ema2)
		{		
			double alphaX = 2.0 / (1.0 + periodX );
			double alphaY = 2.0 / (1.0 + periodY );
			
			return (ema1[bar] * alphaX - ema2[bar] * alphaY) / (alphaX - alphaY);
		}

        private double PMACDlevel(double level, int bar, TimeSeries price, int periodX, int periodY, TimeSeries ema1, TimeSeries ema2)
        {		
			double alphaX = 2.0 / (1.0 + periodX );
			double alphaY = 2.0 / (1.0 + periodY );
			double OneAlphaX = 1.0 - alphaX;
			double OneAlphaY = 1.0 - alphaY;
			
			return (level + ema2[bar] * OneAlphaY -
				ema1[bar] * OneAlphaX) / (alphaX - alphaY);
		}

        private double PMACDzero(int bar, TimeSeries price, int periodX, int periodY, TimeSeries ema1, TimeSeries ema2)
        {		
			return PMACDlevel( 0, bar, price, periodX, periodY, ema1, ema2 );
		}

        //parameterless constructor
        public RevEngMACD() : base()
        {
        }

        //for code based construction
        public RevEngMACD(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static RevEngMACD Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("RevEngMACD", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (RevEngMACD)source.Cache[key];
            RevEngMACD rem = new RevEngMACD(source, period1, period2);
            source.Cache[key] = rem;
            return rem;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("MACD Period 1", ParameterType.Int32, 12);
            AddParameter("MACD Period 2", ParameterType.Int32, 26);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            var FirstValidValue = Math.Max(period1, period2) * 3;

            if (period1 <= 0 || period2 <= 0 || ds.Count == 0 || ds.Count < FirstValidValue)
                return;

            var ema1 = new EMA(ds, period1);
            var ema2 = new EMA(ds, period2);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = PMACDzero(bar, ds, period1, period2, ema1, ema2);               
            }
        }

        public override string Name => "RevEngMACD";

        public override string Abbreviation => "RevEngMACD";

        public override string HelpDescription => "From S&C January 2012 article Reversing MACD by Johnny Dough. Returns the price value required for the MACD to move to a value on the following bar.";

        public override string PaneTag => @"MACD";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}