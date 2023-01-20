using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SVEVolatilityBandUpper : IndicatorBase
    {
        public override string Name => "SVEVolatilityBandUpper";

        public override string Abbreviation => "SVEVolatilityBandUpper";

        public override string HelpDescription => "Upper Band of SVE Volaitility Bands from the August 2013 issue of Stocks & Commodities Magazine.";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.Silver;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Bands;

        public override List<string> Companions => new List<string>() { "SVEVolatilityBandLower" };

        //parameterless constructor
        public SVEVolatilityBandUpper() : base()
        {
        }

        //for code based construction
        public SVEVolatilityBandUpper(BarHistory bars, int bandAverage, int volSumPeriod, double devFactor, double lowBandAdj)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = bandAverage;
            Parameters[2].Value = volSumPeriod;
            Parameters[3].Value = devFactor;
            Parameters[4].Value = lowBandAdj;

            Populate();
        }

        //static method
        public static SVEVolatilityBandUpper Series(BarHistory source, int bandAverage, int volSumPeriod, double devFactor, double lowBandAdj)
        {
            string key = CacheKey("SVEVolatilityBandUpper", bandAverage, volSumPeriod, devFactor, lowBandAdj);
            if (source.Cache.ContainsKey(key))
                return (SVEVolatilityBandUpper)source.Cache[key];
            SVEVolatilityBandUpper sve = new SVEVolatilityBandUpper(source, bandAverage, volSumPeriod, devFactor, lowBandAdj);
            source.Cache[key] = sve;
            return sve;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Band average", ParameterType.Int32, 8);
            AddParameter("Volatility Summing period", ParameterType.Int32, 13);
            AddParameter("Deviation factor", ParameterType.Double, 3.55);
            AddParameter("Low band adjustment", ParameterType.Double, 0.9);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 BandAverage = Parameters[1].AsInt;
            Int32 VolPeriod = Parameters[2].AsInt;
            Double DevFact = Parameters[3].AsDouble;
            Double LowBandAdjust = Parameters[4].AsDouble;

            DateTimes = bars.DateTimes;
            int period = Math.Max(BandAverage, VolPeriod);

            if (period <= 0 || bars.Count == 0)
                return;

            if (bars.Count < Math.Max(BandAverage, VolPeriod))
                return;

            var TypicalPrice = bars.AveragePriceHLC;
            TimeSeries typical = new TimeSeries(DateTimes);
            TimeSeries deviation = new TimeSeries(DateTimes);
            TimeSeries medianaverage = new TimeSeries(DateTimes);
            TimeSeries devhigh = new TimeSeries(DateTimes);
            TimeSeries devlow = new TimeSeries(DateTimes);
            TimeSeries UpperBand = new TimeSeries(DateTimes);

            for (int bar = 2; bar < bars.Count; bar++)
            {
                if (bar < 2)
                {
                    typical[bar] = 0d;
                }
                else
                {
                    // basic volatility in new "typical" data series
                    if (TypicalPrice[bar] >= TypicalPrice[bar - 1])
                        typical[bar] = TypicalPrice[bar] - bars.Low[bar - 1];
                    else
                        typical[bar] = TypicalPrice[bar - 1] - bars.Low[bar];
                }
            }

            // basic deviation based on "typical" over a period and a factor
            deviation = typical.Sum(VolPeriod) / VolPeriod * DevFact;

            // the average deviation high and low band
            devhigh = new EMA(deviation, BandAverage);
            devlow = new EMA(deviation, BandAverage) * LowBandAdjust;

            // the middle average reference
            medianaverage = new EMA(TypicalPrice, BandAverage);

            // show only after it is stable
            EMA emaMedianAvg = new EMA(medianaverage, BandAverage);
            UpperBand = emaMedianAvg + devhigh;
            //LowerBand = emaMedianAvg - devlow;

            for (int bar = Math.Max(BandAverage, VolPeriod); bar < bars.Count; bar++)
            {
                Values[bar] = UpperBand[bar];
            }
        }
    }
}