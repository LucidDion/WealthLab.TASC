using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class LRAdjEMA : IndicatorBase
    {
        //constructors
        public LRAdjEMA() : base()
        {
        }
        public LRAdjEMA(TimeSeries source, int period = 50, int timePeriods = 50, double multiplier = 3.0) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = timePeriods;
            Parameters[3].Value = multiplier;
            Populate();
        }

        //static method
        public static LRAdjEMA Series(TimeSeries source, int period = 50, int timePeriods = 50, double multiplier = 3.0)
        {
            string key = CacheKey("LRAdjEMA", period, timePeriods, multiplier);
            if (source.Cache.ContainsKey(key))
                return (LRAdjEMA)source.Cache[key];
            LRAdjEMA l = new LRAdjEMA(source, period, timePeriods, multiplier);
            source.Cache[key] = l;
            return l;
        }

        //Name
        public override string Name => "Linear Regression Adjusted Exponential Moving Average";

        //abbreviation
        public override string Abbreviation => "LRAdjEMA";

        //help
        public override string HelpDescription => "Linear Regression Adjusted Exponential Moving Average, based on the article by Vitali Apirine in the September 2022 issue of Stocks & Commodities magazine.";

        //plot in source pane
        public override string PaneTag => "Price";

        //color
        public override WLColor DefaultColor => WLColor.BlueViolet;

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("EMA Period", ParameterType.Int32, 50);
            AddParameter("Time Periods", ParameterType.Int32, 50);
            AddParameter("Multiplier", ParameterType.Double, 3.0);
        }

        //populate
        public override void Populate()
        {
            //get parameter values
            TimeSeries source = Parameters[0].AsTimeSeries;
            int emaPeriod = Parameters[1].AsInt;
            int timePeriod = Parameters[2].AsInt;
            double multiplier = Parameters[3].AsDouble;
            DateTimes = source.DateTimes;
            if (source.Count < Math.Max(emaPeriod, timePeriod) + 1)
                return;

            //calculate
            double Mltp1 = 2.0 / (timePeriod + 1.0);
            TimeSeries lr = LR.Series(source, emaPeriod);
            TimeSeries Dist = (lr - source).Abs();
            TimeSeries ST = (Dist - Lowest.Series(Dist, emaPeriod)) / ( Highest.Series(Dist, emaPeriod) - Lowest.Series(Dist, emaPeriod));
            TimeSeries Mltp2 = ST * multiplier;
            TimeSeries Rate = Mltp1 * (1.0 + Mltp2);
            TimeSeries result = new TimeSeries(source.DateTimes, 0);
            double prev = 0;

            for (int n = 0; n < DateTimes.Count; n++)
            {
                //In the case of cum(1) it essentially takes a cumulative sum of 1 for each price plot in the chart, giving you the total number of bars at any given point in the chart.
                if (n <= emaPeriod * 2)
                    prev = source[n];
                else
                    //If(Cum(1)=Periods+1,C,PREV+Rate*(C-PREV));
                    prev = prev + Rate[n] * (source[n] - prev);

                result[n] = prev;
                base.Values[n] = result[n];
            }
            PrefillNan(emaPeriod + timePeriod);
        }
    }
}