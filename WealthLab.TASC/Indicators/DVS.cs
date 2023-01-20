using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //DVS Indicator class
    public class DVS : IndicatorBase
    {
        //parameterless constructor
        public DVS() : base()
        {
        }

        //for code based construction
        public DVS(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static DVS Series(TimeSeries source, int period)
        {
            string key = CacheKey("DVS", period);
            if (source.Cache.ContainsKey(key))
                return (DVS)source.Cache[key];
            DVS dvs = new DVS(source, period);
            source.Cache[key] = dvs;
            return dvs;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 50);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            if (period < 4) period = 4;
            int halfperiod = period / 2;
            var dvs = (ds >> (halfperiod / 2)) / FastSMA.Series(ds, halfperiod);
            dvs = new SMA(new StdDev(dvs, period), halfperiod);

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + halfperiod - 1 + period - 1 + halfperiod - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //     Values[bar] = 0;

            //Rest of series
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if(bar >= FirstValidValue)
                    Values[bar] = 100 * dvs[bar];
                else
                    Values[bar] = Double.NaN;
            }
        }

        public override string Name => "DVS";

        public override string Abbreviation => "DVS";

        public override string HelpDescription => @"Dynamic Volatility Sigma from the May 2005 issue of Stocks & Commodities magazine. DVS is used in the calculation of the Universal Cycle Indicator and is the standard deviation of minor plus sub-minor price oscillations with respect to a minor term centered moving average. The centered averages introduce a half-cycle minor term lag in the standard deviation parameter.";

        public override string PaneTag => @"DVS";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }    
}