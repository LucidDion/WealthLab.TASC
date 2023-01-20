﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RainbowOsc : IndicatorBase
    {
        //parameterless constructor
        public RainbowOsc() : base()
        {
        }

        //for code based construction
        public RainbowOsc(TimeSeries source, Int32 period, Int32 k)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = k;

            Populate();
        }

        //static method
        public static RainbowOsc Series(TimeSeries source, int period, int k)
        {
            string key = CacheKey("RainbowOsc", period, k);
            if (source.Cache.ContainsKey(key))
                return (RainbowOsc)source.Cache[key];
            RainbowOsc ro = new RainbowOsc(source, period, k);
            source.Cache[key] = ro;
            return ro;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 2);
            AddParameter("Levels", ParameterType.Int32, 10);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 levels = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(period, levels);
            if (FirstValidValue <= 1) return;

            if (FirstValidValue > ds.Count || FirstValidValue < 0)
                FirstValidValue = ds.Count;
            if (ds.Count < period)
                return;

            if (ds.Count >= period)
            {
                var lst = new List<TimeSeries>();
                lst.Add(new SMA(ds, period));

                for (int i = 1; i < levels; i++)
                {
                    var d = new SMA(lst[lst.Count - 1], period);
                    d.Description = string.Format("Rainbow({0})", i);
                    lst.Add(d);
                }

                var HiPrice = new Highest(ds, levels);
                var LoPrice = new Lowest(ds, levels);
                var AvgVal = new TimeSeries(DateTimes);

                for (int bar = 0; bar < ds.Count; bar++)
                {
                    AvgVal[bar] = Values[bar] = 0d;
                }

                for (int bar = period; bar < ds.Count; bar++)
                {
                    double avg = 0;
                    foreach (TimeSeries d in lst)
                    {
                        avg += d[bar];
                    }
                    AvgVal[bar] = avg / levels;
                }

                var RainbowOsc = 100 * ((ds - AvgVal) / (HiPrice - LoPrice));

                for (int bar = FirstValidValue; bar < ds.Count; bar++)
                {
                    Values[bar] = RainbowOsc[bar];
                }
            }
            PrefillNan(FirstValidValue);
        }

        public override string Name => "RainbowOsc";

        public override string Abbreviation => "RainbowOsc";

        public override string HelpDescription => "Rainbow Oscillator, developed by Mel Widner (Stocks and Commodities magazine 07/1997), is a trend changing indicator.";

        public override string PaneTag => @"RainbowOsc";

        public override WLColor DefaultColor => WLColor.DarkBlue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}