using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RSVAEMA : IndicatorBase
    {
        public override string Name => "Relative Strength Volume-Adjusted Exponential Moving Average";
        public override string Abbreviation => "RSVAEMA";
        public override string HelpDescription => "Relative Strength Volume-Adjusted Exponential Moving Average, based on the article by Vitali Apirine in the October 2022 issue of Stocks & Commodities magazine.";
        public override string PaneTag => @"Price";
        public override WLColor DefaultColor => WLColor.Green;
        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

        public RSVAEMA()
        {
        }
        public RSVAEMA(TimeSeries source, int timePeriod = 10, int lookbackPeriod = 10, int multplier = 10)
        {
            base.Parameters[0].Value = source;
            base.Parameters[1].Value = timePeriod;
            base.Parameters[2].Value = lookbackPeriod;
            base.Parameters[3].Value = multplier;
            this.Populate();
        }

        //static method
        public static RSVAEMA Series(TimeSeries source, int timePeriod, int lookbackPeriod, int multplier)
        {
            string key = CacheKey("RSVAEMA", timePeriod, lookbackPeriod, multplier);
            if (source.Cache.ContainsKey(key))
                return (RSVAEMA)source.Cache[key];
            RSVAEMA r = new RSVAEMA(source, timePeriod, lookbackPeriod, multplier);
            source.Cache[key] = r;
            return r;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            base.AddParameter("Time Periods", ParameterType.Int32, 10);
            base.AddParameter("Lookback Period", ParameterType.Int32, 10);
            base.AddParameter("Multiplier", ParameterType.Int32, 10);
        }
        public override void Populate()
        {
            TimeSeries ts = base.Parameters[0].AsTimeSeries;
            int Periods = base.Parameters[1].AsInt;
            int Pds = base.Parameters[2].AsInt;
            int Mltp = base.Parameters[3].AsInt;
            double Mltp1 = 2.0 / (Periods + 1.0);
            this.DateTimes = ts.DateTimes;

            int FirstValidValue = Math.Max(Periods, Pds);
            if (ts.Count < FirstValidValue)
            {
                return;
            }

            TimeSeries VolUpDay = new TimeSeries(ts.DateTimes, 0);
            TimeSeries VolDwnDay = new TimeSeries(ts.DateTimes, 0);
            TimeSeries RS = new TimeSeries(ts.DateTimes, 0);

            for (int i = 1; i < ts.DateTimes.Count; i++)
            {
                if (ts[i] > ts[i - 1])
                    VolUpDay[i] = ts[i] - ts[i - 1];
                if (ts[i] < ts[i - 1])
                    VolDwnDay[i] = ts[i - 1] - ts[i];
            }

            RS = (EMA.Series(VolUpDay, Pds) - EMA.Series(VolDwnDay, Pds)).Abs() /
                (EMA.Series(VolUpDay, Pds) + EMA.Series(VolDwnDay, Pds) + 0.00001);

            RS *= Mltp;
            var Rate = Mltp1 * (1 + RS);
            TimeSeries result = new TimeSeries(ts.DateTimes, 0);

            double prev = ts[0];
            for (int i = 0; i < ts.DateTimes.Count; i++)
            {
                //In the case of cum(1) it essentially takes a cumulative sum of 1 for each price plot in the chart, giving you the total number of bars at any given point in the chart.
                if (i <= Periods + 1)
                    prev = ts[i];
                else
                    //If(Cum(1)=Periods+1,C,PREV+Rate*(C-PREV));
                    prev = prev + Rate[i] * (ts[i] - prev);

                result[i] = prev;
                base.Values[i] = result[i];
            }
            PrefillNan(FirstValidValue);
        }
    }
}