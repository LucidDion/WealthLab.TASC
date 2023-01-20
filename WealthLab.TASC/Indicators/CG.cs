using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //CG Indicator class
    public class CG : IndicatorBase
    {
        //parameterless constructor
        public CG() : base()
        {
        }

        //for code based construction
        public CG(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }
        
        //static method
        public static CG Series(TimeSeries source, int period)
        {
            string key = CacheKey("CG", period);
            if (source.Cache.ContainsKey(key))
                return (CG)source.Cache[key];
            CG cg = new CG(source, period);
            source.Cache[key] = cg;
            return cg;
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
            if (period < 1 || period > ds.Count + 1) period = ds.Count + 1;

            if (period <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + period - 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes, and begin accumulating values
            //for (int bar = 0; bar < ds.FirstValidValue; bar++)
            //    Values[bar] = 0;
            double WSum = 0, Sum = 0;
            for (int bar = FirstValidValue; bar < FirstValidValue; bar++)
            {
                WSum += ds[bar] * (period - bar);
                Sum += ds[bar];
                //Values[bar] = 0;
            }

            //Average rest of series
            for (int bar = period - 1; bar < ds.Count; bar++)   
            {
                Sum += ds[bar];
                WSum += ds[bar];
                Values[bar] = -WSum / Sum;
                Sum -= ds[bar - period + 1];
                WSum += Sum - ds[bar - period + 1] * period;
            }
        }



        public override string Name => "CG";

        public override string Abbreviation => "CG";

        public override string HelpDescription => @"This is the Center of Gravity oscillator by John Ehlers from the May 2002 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"CG";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;

    }
}