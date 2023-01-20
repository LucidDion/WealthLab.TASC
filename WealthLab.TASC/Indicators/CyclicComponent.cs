using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //CyclicComponent Indicator class
    public class CyclicComponent : IndicatorBase
    {
        //parameterless constructor
        public CyclicComponent() : base()
        {
        }

        //for code based construction
        public CyclicComponent(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static CyclicComponent Series(TimeSeries source, int period)
        {
            string key = CacheKey("CyclicComponent", period);
            if (source.Cache.ContainsKey(key))
                return (CyclicComponent)source.Cache[key];
            CyclicComponent cc = new CyclicComponent(source, period);
            source.Cache[key] = cc;
            return cc;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 30);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            //Avoid exception errors
            if (period < 1 || period > ds.Count) period = ds.Count;
            if (period < 5) period = 5; // avoid ending with Cos(2*PI/Period) <= 0

            if (period <= 0 || ds.Count == 0)
                return;

            var alpha = (1 - Math.Sin(2 * Math.PI / period)) / Math.Cos(2 * Math.PI / period);
            var w = (1 + alpha) / 2; 
              
            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            double HP = 0, HP1 = 0, HP2 = 0, HP3 = 0;
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                HP3 = HP2;
                HP2 = HP1;
                HP1 = HP;
                HP = alpha * HP + w * (ds[bar] - ds[bar - 1]);
                Values[bar] = (HP + 2 * (HP1 + HP2) + HP3) / 6;
            }
        }



        public override string Name => "CyclicComponent";

        public override string Abbreviation => "CyclicComponent";

        public override string HelpDescription => @"For a comprehensive explanation please refer to John Ehlers´ article 'Modeling the Market = Building Trading Strategies' in the August 2006 issue of Stocks and Commodities Magazine.";

        public override string PaneTag => @"CyclicComponent";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }
}