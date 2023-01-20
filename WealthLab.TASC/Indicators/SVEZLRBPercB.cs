using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SVEZLRBPercB : IndicatorBase
    {
        //parameterless constructor
        public SVEZLRBPercB() : base()
        {
        }

        //for code based construction
        public SVEZLRBPercB(BarHistory bars, Int32 smooth, Int32 sdPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = smooth;
            Parameters[2].Value = sdPeriod;

            Populate();
        }

        //static method
        public static SVEZLRBPercB Series(BarHistory source, int smooth, int sdPeriod)
        {
            string key = CacheKey("SVEZLRBPercB", smooth, sdPeriod);
            if (source.Cache.ContainsKey(key))
                return (SVEZLRBPercB)source.Cache[key];
            SVEZLRBPercB sve = new SVEZLRBPercB(source, smooth, sdPeriod);
            source.Cache[key] = sve;
            return sve;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("Smoothing Period", ParameterType.Int32, 3);
            AddParameter("StdDev Period", ParameterType.Int32, 18);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 smooth = Parameters[1].AsInt;
            Int32 sdPeriod = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(smooth, sdPeriod);

            if (period <= 0 || DateTimes.Count == 0 || bars.Count < period)
                return;

            FastSMA sma = FastSMA.Series(bars.Close, 2);
            TimeSeries sma1 = sma * 5;
            TimeSeries sma2 = FastSMA.Series(sma, 2) * 4;
            TimeSeries sma3 = FastSMA.Series(FastSMA.Series(sma, 2), 2) * 3;
            TimeSeries sma4 = FastSMA.Series(FastSMA.Series(FastSMA.Series(sma, 2), 2), 2) * 2;
            TimeSeries sma5 = FastSMA.Series(FastSMA.Series(FastSMA.Series(FastSMA.Series(sma, 2), 2), 2), 2);
            TimeSeries sma6 = FastSMA.Series(sma5, 2);
            TimeSeries sma7 = FastSMA.Series(sma6, 2);
            TimeSeries sma8 = FastSMA.Series(sma7, 2);
            TimeSeries sma9 = FastSMA.Series(sma8, 2);
            TimeSeries sma10 = FastSMA.Series(sma9, 2);
            TimeSeries Rainbow = (sma1 + sma2 + sma3 + sma4 + sma5 + sma6 + sma7 + sma8 + sma9 + sma10) / 20;

            TimeSeries ema1 = EMA.Series(Rainbow, smooth);
            TimeSeries ema2 = EMA.Series(ema1, smooth);
            TimeSeries diff = ema1 - ema2;
            TimeSeries ZLRB = ema1 + diff;
            TEMA_TASC tema = TEMA_TASC.Series(ZLRB, smooth);
            StdDev sd = StdDev.Series(tema, sdPeriod);
            WMA wma = WMA.Series(tema, sdPeriod);
            var PB = (tema + sd * 2 - wma) / (sd * 4) * 100;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                base[bar] = PB[bar];
            }
        }
        
        public override string Name => "SVEZLRBPercB";

        public override string Abbreviation => "SVEZLRBPercB";

        public override string HelpDescription => "Sylvain Vervoort's SVEZLRBPercB indicator from September 2013 issue of Stocks & Commodities magazine is a Smoothed zero-lagging Percent b indicator on rainbow price series.";

        public override string PaneTag => @"RainbowPane";   // Share pane between SVEZLRBPercB and SVERBStochK

        public override WLColor DefaultColor => WLColor.DodgerBlue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }    
}