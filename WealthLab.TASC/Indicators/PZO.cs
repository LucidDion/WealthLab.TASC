﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class PZO : IndicatorBase
    {
        //parameterless constructor
        public PZO() : base()
        {
        }

        //for code based construction
        public PZO(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static PZO Series(BarHistory source, int period)
        {
            string key = CacheKey("PZO", period);
            if (source.Cache.ContainsKey(key))
                return (PZO)source.Cache[key];
            PZO p = new PZO(source, period);
            source.Cache[key] = p;
            return p;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var R = new TimeSeries(DateTimes);
            var TV = new EMA(bars.Close, period);
            
            for (int bar = period; bar < bars.Count; bar++)
            {
                R[bar] = Math.Sign(bars.Close[bar] - bars.Close[bar - 1]) * bars.Close[bar];
            }
            var VP = new EMA(R, period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                if (TV[bar] != 0)
                    Values[bar] = 100 * VP[bar] / TV[bar];
            }
        }

        public override string Name => "PZO";

        public override string Abbreviation => "PZO";

        public override string HelpDescription => "The complementary Price Zone Oscillator (see VZO) by W.Khalil and D.Steckler originates from S&C June 2011 Traders' Tips.";

        public override string PaneTag => "PZO";

        public override WLColor DefaultColor => WLColor.DarkBlue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}