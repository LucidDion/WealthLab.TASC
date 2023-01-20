using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    /// <summary>
    /// TEMA (Triple-Smoothed Exponential Moving Average) Indicator
    /// </summary>
    public class TEMA_TASC : IndicatorBase
    {
        //parameterless constructor
        public TEMA_TASC() : base()
        {
        }

        //for code based construction
        public TEMA_TASC(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static TEMA_TASC Series(TimeSeries source, int period)
        {
            string key = CacheKey("TEMA_TASC", period);
            if (source.Cache.ContainsKey(key))
                return (TEMA_TASC)source.Cache[key];
            TEMA_TASC tt = new TEMA_TASC(source, period);
            source.Cache[key] = tt;
            return tt;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Rest of series
            var ema1 = EMA.Series(ds, period);
            var ema2 = EMA.Series(ema1, period);
            var ema3 = EMA.Series(ema2, period);

            for (int bar = period; bar < ds.Count; bar++)
            {
                Values[bar] = 3 * ema1[bar] - 3 * ema2[bar] + ema3[bar];
            }            
        }



        public override string Name => "TEMA_TASC";

        public override string Abbreviation => "TEMA_TASC";

        public override string HelpDescription => @"TEMA is the Triple-smoothed Exponential Moving Average based on TECHNICAL ANALYSIS FROM A TO Z, 2nd Ed., pg. 328-330.";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.DarkMagenta;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }    
}