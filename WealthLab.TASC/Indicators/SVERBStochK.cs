using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SVERBStochK : IndicatorBase
    {
        //parameterless constructor
        public SVERBStochK() : base()
        {
        }

        //for code based construction
        public SVERBStochK(BarHistory bars, Int32 stochPeriod, Int32 smoothPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = stochPeriod;
            Parameters[2].Value = smoothPeriod;

            Populate();
        }

        //static method
        public static SVERBStochK Series(BarHistory source, int stochPeriod, int smoothPeriod)
        {
            string key = CacheKey("SVERBStochK", stochPeriod, smoothPeriod);
            if (source.Cache.ContainsKey(key))
                return (SVERBStochK)source.Cache[key];
            SVERBStochK sve = new SVERBStochK(source, stochPeriod, smoothPeriod);
            source.Cache[key] = sve;
            return sve;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("StochK Period", ParameterType.Int32, 30);
            AddParameter("Smoothing K Period", ParameterType.Int32, 3);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 periodK = Parameters[1].AsInt;
            Int32 smoothK = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(periodK, smoothK);

            if (period <= 0 || DateTimes.Count == 0)
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

            TimeSeries RBC = (Rainbow + bars.AveragePriceHLC) / 2;
            TimeSeries nom = RBC - Lowest.Series(bars.Low, periodK);
            TimeSeries den = Highest.Series(bars.High, periodK) - Lowest.Series(RBC, periodK);

            var fastK = new TimeSeries(DateTimes);
            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar >= periodK)
                    fastK[bar] = (Math.Min(100, Math.Max(0, 100 * nom[bar] / den[bar])));
                else
                    fastK[bar] = 0d;
            }

            var K = FastSMA.Series(fastK, smoothK);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                Values[bar] = K[bar];
            }
        }

        public override string Name => "SVERBStochK";

        public override string Abbreviation => "SVERBStochK";

        public override string HelpDescription => "Sylvain Vervoort's SVERBStochK indicator from September 2013 issue of Stocks & Commodities magazine is a Stochastic K oscillator using the Rainbow data series.";

        public override string PaneTag => @"RainbowPane";   // Share pane between SVEZLRBPercB and SVERBStochK

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }    
}