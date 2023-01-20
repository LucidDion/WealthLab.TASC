using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //TCFPlus Indicator class
    public class TCFPlus : IndicatorBase
    {
        //parameterless constructor
        public TCFPlus() : base()
        {
        }

        //for code based construction
        public TCFPlus(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static TCFPlus Series(TimeSeries source, int period)
        {
            string key = CacheKey("TCFPlus", period);
            if (source.Cache.ContainsKey(key))
                return (TCFPlus)source.Cache[key];
            TCFPlus t = new TCFPlus(source, period);
            source.Cache[key] = t;
            return t;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 35);
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

            //Build intermediate series
            var ChangePlus = new TimeSeries(DateTimes);
            var CFMinus = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= ds.FirstValidIndex + 1)
                {
                    double xChange = ds[bar] - ds[bar - 1];
                    if (xChange > 0)
                    {
                        ChangePlus[bar] = xChange;
                        CFMinus[bar] = 0;
                    }
                    else
                    {
                        ChangePlus[bar] = 0;
                        CFMinus[bar] = CFMinus[bar - 1] - xChange;
                    }
                }
                else
                {
                    CFMinus[bar] = 0d; ChangePlus[bar] = 0d;
                }
            }
            var SumChangePlus = ChangePlus.Sum(period);
            var SumCFMinus = CFMinus.Sum(period);

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = 0d;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = SumChangePlus[bar] - SumCFMinus[bar];
            PrefillNan(FirstValidValue);
        }

        public override string Name => "TCFPlus";

        public override string Abbreviation => "TCFPlus";

        public override string HelpDescription => "Trend Continuation - Indicator from the February 2002 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"TCF";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}