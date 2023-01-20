using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //Fisher Indicator class
    public class Fisher : IndicatorBase                
    {
        //parameterless constructor
        public Fisher() : base()
        {
        }

        //for code based construction
        public Fisher(TimeSeries source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static Fisher Series(TimeSeries source, int period)
        {
            string key = CacheKey("Fisher", period);
            if (source.Cache.ContainsKey(key))
                return (Fisher)source.Cache[key];
            Fisher f = new Fisher(source, period);
            source.Cache[key] = f;
            return f;
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

            if (period <= 0 || ds.Count == 0)
                return;
            
            //Avoid exception errors
            if (period < 1 || period > ds.Count + 1) period = ds.Count + 1;

            var Lo = new Lowest(ds, period);
            var Hi = new Highest(ds, period);
            double Value1 = 0, Value = 0;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Value1 = 0.67 * Value1;
                if (Lo[bar] < Hi[bar])
                    Value1 += 0.33 * (2 * (ds[bar] - Lo[bar]) / (Hi[bar] - Lo[bar]) - 1);
                Value = 0.5 * Value;
                Value += 0.5 * Math.Log((1 + Value1) / (1 - Value1));
                Values[bar] = Value;
            }
            PrefillNan(FirstValidValue);
        }

        public override string Name => "Fisher";

        public override string Abbreviation => "Fisher";

        public override string HelpDescription => "John Ehlers' Fisher Transform indicator as presented in the November 2002 issue of Stocks & Commodities magazine.";
    
        public override string PaneTag => @"Fisher";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }
}