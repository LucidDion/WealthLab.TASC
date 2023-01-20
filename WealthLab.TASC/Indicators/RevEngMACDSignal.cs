using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC 
{
    public class RevEngMACDSignal : IndicatorBase
    {
        private double PMACDeq(int bar, TimeSeries price, int periodX, int periodY)
		{		
			double alphaX = 2.0 / (1.0 + periodX );
			double alphaY = 2.0 / (1.0 + periodY );

            var ema1 = new EMA(price, periodX);
            var ema2 = new EMA(price, periodY);

            return (ema1[bar] * alphaX - ema2[bar] * alphaY) / (alphaX - alphaY);
		}

        // returns price where sMACD is equal to previous bar sMACD
        private double PsMACDeq(int bar, TimeSeries price, int periodX, int periodY)
        {
            return (periodX * price[periodY + 1] - periodY * price[periodX + 1]) / (periodX - periodY);
        }

        private double PMACDlevel(double level, int bar, TimeSeries price, int periodX, int periodY)
		{		
			double alphaX = 2.0 / (1.0 + periodX );
			double alphaY = 2.0 / (1.0 + periodY );
			double OneAlphaX = 1.0 - alphaX;
			double OneAlphaY = 1.0 - alphaY;

            var ema1 = new EMA(price, periodX);
            var ema2 = new EMA(price, periodY);

            return (level + ema2[bar] * OneAlphaY - ema1[bar] * OneAlphaX) / (alphaX - alphaY);
		}

        // returns price where MACD cross signal line or MACD histogram crosses 0
        private double PMACDsignal(int bar, TimeSeries price, int periodX, int periodY, int periodZ)
        {
            double alphaX = 2.0 / (1.0 + periodX);
            double alphaY = 2.0 / (1.0 + periodY);
            double alphaZ = 2.0 / (1.0 + periodZ);
            double OneAlphaX = 1.0 - alphaX;
            double OneAlphaY = 1.0 - alphaY;
            double OneAlphaZ = 1.0 - alphaZ;    // leftover? not used in the formula
            var ema1 = new EMA(price, periodX);
            var ema2 = new EMA(price, periodY);
            var macdex = ema1 - ema2;
            var macdexSignal = new EMA(macdex, periodZ);
            double MACDvalue = macdex[bar];
            double MACDsignal = macdexSignal[bar];

            return (MACDsignal - ema1[bar] * OneAlphaX + ema2[bar] * OneAlphaY) / (alphaX - alphaY);
        }

        private double PMACDzero(int bar, TimeSeries price, int periodX, int periodY)
		{		
			return PMACDlevel( 0, bar, price, periodX, periodY );
		}

        // returns price where sMACD cross 0
        private double PsMACDzero(int bar, TimeSeries price, int periodX, int periodY)
        {
            double result = (periodX * periodY * (FastSMA.Value(bar, price, periodX) - FastSMA.Value(bar, price, periodY)) +
                       periodX * price[periodY + 1] -
                       periodY * price[periodX + 1]) /
                        (periodX - periodY);

            return result;
        }

        //parameterless constructor
        public RevEngMACDSignal() : base()
        {
        }

        //for code based construction
        public RevEngMACDSignal(TimeSeries source, Int32 period1, Int32 period2, Int32 period3, bool useSMA)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = period3;
            Parameters[4].Value = useSMA;

            Populate();
        }

        //static method
        public static RevEngMACDSignal Series(TimeSeries source, int period1, int period2, int period3, bool useSMA)
        {
            string key = CacheKey("RevEngMACDSignal", period1, period2, period3, useSMA);
            if (source.Cache.ContainsKey(key))
                return (RevEngMACDSignal)source.Cache[key];
            RevEngMACDSignal rems = new RevEngMACDSignal(source, period1, period2, period3, useSMA);
            source.Cache[key] = rems;
            return rems;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("MACD Period 1", ParameterType.Int32, 12);
            AddParameter("MACD Period 2", ParameterType.Int32, 26);
            AddParameter("MACD Signal Period", ParameterType.Int32, 9);
            AddParameter("Use SMA?", ParameterType.Boolean, true);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 period3 = Parameters[3].AsInt;
            bool useSMA = Parameters[4].AsBoolean;

            DateTimes = ds.DateTimes;
            var period = new List<int> { period1, period2, period3 }.Max();

            if (period <= 0 || ds.Count == 0)
                return;

            //var FirstValidValue = Math.Max(period1, period2) * 3;

            var psms = new TimeSeries(DateTimes);
            if (useSMA)
                psms = new PsMACDsignal(ds, period1, period2, period3);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = useSMA == false ? PMACDsignal(bar, ds, period1, period2, period3) : psms[bar];
            }
        }

        public override string Name => "RevEngMACDSignal";

        public override string Abbreviation => "RevEngMACDSignal";

        public override string HelpDescription => "From S&C November 2013 article 'Reversing MACD: The Sequel' by Johnny Dough. Returns the price value required for the MACD line and signal line to cross on the next bar.";

        public override string PaneTag => @"PsMACDsignal";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}