using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class VMACDH : IndicatorBase
    {
        //parameterless constructor
        public VMACDH() : base()
        {
        }

        //for code based construction
        public VMACDH(BarHistory bars, Int32 shortPeriod, Int32 longPeriod, Int32 signalPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = shortPeriod;
            Parameters[2].Value = longPeriod;
            Parameters[3].Value = signalPeriod;

            Populate();
        }

        //static method
        public static VMACDH Series(BarHistory source, int shortPeriod, int longPeriod, int signalPeriod)
        {
            string key = CacheKey("VMACDH", shortPeriod, longPeriod, signalPeriod);
            if (source.Cache.ContainsKey(key))
                return (VMACDH)source.Cache[key];
            VMACDH v = new VMACDH(source, shortPeriod, longPeriod, signalPeriod);
            source.Cache[key] = v;
            return v;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("Short period", ParameterType.Int32, 12);
            AddParameter("Long period", ParameterType.Int32, 26);
            AddParameter("Signal period", ParameterType.Int32, 9);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 shortPeriod = Parameters[1].AsInt;
            Int32 longPeriod = Parameters[2].AsInt;
            Int32 signalPeriod = Parameters[3].AsInt;

            DateTimes = bars.DateTimes;
            var period = new List<int> { shortPeriod, longPeriod, signalPeriod }.Max();
            if (period <= 0 || DateTimes.Count == 0)
                return;

            var _expS = 2d / (1d + shortPeriod);
            var _expL = 2d / (1d + longPeriod);
            var _expG = 2d / (1d + signalPeriod);

            //Avoid exceptions
            if (shortPeriod < 1 || longPeriod < 1 || shortPeriod > bars.Count || longPeriod > bars.Count ) 
                longPeriod = bars.Count + 1;

            //Assign first bar that contains indicator data
            var FirstValidValue = longPeriod + signalPeriod;
            if (FirstValidValue > bars.Count) FirstValidValue = bars.Count;

            // Calculate each ema on the fly separately to conserve memory; init with SMA
            var pv = bars.Close * bars.Volume;

            //double emaS = Sum.Value(shortPeriod - 1, pv, shortPeriod) / shortPeriod;
            //double emaL = Sum.Value(longPeriod - 1, pv, longPeriod) / longPeriod;
            //double emaVS = Sum.Value(shortPeriod - 1, bars.Volume, shortPeriod) / shortPeriod;
            //double emaVL = Sum.Value(longPeriod - 1, bars.Volume, longPeriod) / longPeriod;

            //// continue calculation for the short period
            //for (int bar = shortPeriod; bar < longPeriod; bar++)
            //{
            //    emaS += _expS * (pv[bar] - emaS);
            //    emaVS += _expS * (bars.Volume[bar] - emaVS);                
            //}
            //var vmacd = new TimeSeries(DateTimes);

            //for (int bar = 0; bar < bars.Count; bar++)
            //{
            //    vmacd[bar] = 0d;
            //}   

            //for (int bar = longPeriod; bar < bars.Count; bar++)
            //{
            //    emaS += _expS * (pv[bar] - emaS);
            //    emaVS += _expS * (bars.Volume[bar] - emaVS);

            //    emaL += _expL * (pv[bar] - emaL);
            //    emaVL += _expL * (bars.Volume[bar] - emaVL);

            //    vmacd[bar] = emaS/emaVS - emaL/emaVL;
            //    double vv = emaS / emaVS - emaL / emaVL;   
            //}

            //// create signal line and EMACDH
            //var signalLine = new TimeSeries(DateTimes);
            //double sig = Sum.Value(signalPeriod + longPeriod - 1, vmacd, signalPeriod) / signalPeriod;

            //for (int bar = signalPeriod + longPeriod; bar < bars.Count; bar++)
            //{
            //    sig += _expG * (vmacd[bar] - sig);
            //    Values[bar] = vmacd[bar] - sig;
            //    double mm = vmacd[bar] - sig;
            //}

            TimeSeries shortMA = new EMA(pv, shortPeriod) / new EMA(bars.Volume, shortPeriod);
			TimeSeries longMA = new EMA(pv, longPeriod)/new EMA(bars.Volume, longPeriod);			
			TimeSeries vmacd = shortMA - longMA;
			TimeSeries sigline = new EMA(vmacd, signalPeriod);
			TimeSeries vhist = vmacd - sigline;
            for (int bar = 0; bar < bars.Count; bar++)
                Values[bar] = vhist[bar];
			//vhist.Description = "VMACDH(" + sPer + "," + lPer + "," + sigPer + ")";

        }


        public override string Name => "VMACDH";

        public override string Abbreviation => "VMACDH";

        public override string HelpDescription => "Volume-Weighted MACD Histogram from the October 2009 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"VMACDH";

        public override WLColor DefaultColor => WLColor.Maroon;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Histogram;        
    }
}