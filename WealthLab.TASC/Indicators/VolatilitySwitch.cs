﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class VolatilitySwitch : IndicatorBase
    {
        //parameterless constructor
        public VolatilitySwitch() : base()
        {
            OverboughtLevel = 0.5;
            OversoldLevel = 0.499;
        }

        //for code based construction
        public VolatilitySwitch(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            OverboughtLevel = 0.5;
            OversoldLevel = 0.499;
            Populate();
        }

        //static method
        public static VolatilitySwitch Series(TimeSeries source, int period)
        {
            string key = CacheKey("VolatilitySwitch", period);
            if (source.Cache.ContainsKey(key))
                return (VolatilitySwitch)source.Cache[key];
            VolatilitySwitch vs = new VolatilitySwitch(source, period);
            source.Cache[key] = vs;
            return vs;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 21);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            if (ds.Count < period) return;

            ROC dailyChange = new ROC(ds, 1);
            var histVola = new StdDev(dailyChange, period) / 100d; // Excel STDEV is based on sample
            var voltSwitch = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= period)
                {
                    int cnt = 0;

                    for (int i = bar; i >= bar - period; i--)
                    {
                        if (histVola[i] <= histVola[bar])
                            cnt++;
                    }

                    Values[bar] = (double)cnt / (double)period;
                }
                else
                    Values[bar] = 0d;                
            }
            PrefillNan(period);
        }

        public override string Name => "VolatilitySwitch";

        public override string Abbreviation => "VolatilitySwitch";

        public override string HelpDescription => "Based on an article by Ron McEwan, published in the February 2013 issue of Stocks and Commodities Magazine. The Volatility (Regime) Switch indicator is a method for adapting a trading strategy when the market changes from a trending mode to a mean reverting one.";

        public override string PaneTag => @"VolatilitySwitch";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}