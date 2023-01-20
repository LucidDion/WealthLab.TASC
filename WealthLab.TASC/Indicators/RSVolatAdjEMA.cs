using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RSVolatAdjEMA : IndicatorBase
    {
        public override string Name => "Adaptive Exponential Moving Average";
        public override string Abbreviation => "RSVolatAdjEMA";
        public override string HelpDescription => "Relative Strength Volatility-Adjusted Exponential Moving Average, based on the article by Vitali Apirine in the March 2022 issue of Stocks & Commodities magazine.";
        public override string PaneTag => "Price";
        public override WLColor DefaultColor => WLColor.DarkBlue;

        public RSVolatAdjEMA()
        {
        }
        public RSVolatAdjEMA(BarHistory source, string vixSymbol, int timePeriod = 10, int lookbackPeriod = 10, int multplier = 10)
        {
            base.Parameters[0].Value = source;
            base.Parameters[1].Value = vixSymbol;
            base.Parameters[2].Value = timePeriod;
            base.Parameters[3].Value = lookbackPeriod;
            base.Parameters[4].Value = multplier;
            this.Populate();
        }

        //static method
        public static RSVolatAdjEMA Series(BarHistory source, string vixSymbol, int timePeriod, int lookbackPeriod, int multplier)
        {
            string key = CacheKey("RSVolatAdjEMA", vixSymbol, timePeriod, lookbackPeriod, multplier);
            if (source.Cache.ContainsKey(key))
                return (RSVolatAdjEMA)source.Cache[key];
            RSVolatAdjEMA r = new RSVolatAdjEMA(source, vixSymbol, timePeriod, lookbackPeriod, multplier);
            source.Cache[key] = r;
            return r;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            Parameter p = base.AddParameter("VIX symbol", ParameterType.String, "^VIX");
            p.Uppercase = true;
            base.AddParameter("Time Periods", ParameterType.Int32, 10);
            base.AddParameter("Lookback Period", ParameterType.Int32, 10);
            base.AddParameter("Multiplier", ParameterType.Int32, 10);
        }
        public override void Populate()
        {
            BarHistory bars = base.Parameters[0].AsBarHistory;
            TimeSeries VolatC = SymbolData.Series(bars, base.Parameters[1].AsString, PriceComponent.Close);
            int Periods = base.Parameters[2].AsInt;
            int Pds = base.Parameters[3].AsInt;
            int Mltp = base.Parameters[4].AsInt;
            double Mltp1 = 2.0 / (Periods + 1.0);
            this.DateTimes = bars.DateTimes;

            int FirstValidValue = Math.Max(Periods, Pds);
            if (bars.Count < FirstValidValue)
            {
                return;
            }

            TimeSeries VolatUpDay = new TimeSeries(bars.DateTimes, 0);
            TimeSeries VolatDwnDay = new TimeSeries(bars.DateTimes, 0);
            TimeSeries RS = new TimeSeries(bars.DateTimes, 0);

            for (int i = 1; i < bars.DateTimes.Count; i++)
            {
                if (bars.Close[i] > bars.Close[i - 1])
                    VolatUpDay[i] = VolatC[i];
                if (bars.Close[i] < bars.Close[i - 1])
                    VolatDwnDay[i] = VolatC[i];
            }

            RS = (EMA.Series(VolatUpDay, Pds) - EMA.Series(VolatDwnDay, Pds)).Abs() /
                (EMA.Series(VolatUpDay, Pds) + EMA.Series(VolatDwnDay, Pds) + 0.00001);

            RS *= Mltp;
            var Rate = Mltp1 * (1 + RS);
            TimeSeries result = new TimeSeries(bars.DateTimes, 0);

            double prev = bars.Close[0];
            for (int i = 0; i < bars.DateTimes.Count; i++)
            {
                //In the case of cum(1) it essentially takes a cumulative sum of 1 for each price plot in the chart, giving you the total number of bars at any given point in the chart.
                if (i <= Periods + 1)
                    prev = bars.Close[i];
                else
                    //If(Cum(1)=Periods+1,C,PREV+Rate*(C-PREV));
                    prev = prev + Rate[i] * (bars.Close[i] - prev);

                result[i] = prev;
                base.Values[i] = result[i];
            }
            PrefillNan(FirstValidValue);
        }
    }
}
