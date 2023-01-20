using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class VZO : IndicatorBase
    {
        //parameterless constructor
        public VZO() : base()
        {
        }

        //for code based construction
        public VZO(BarHistory bars, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static VZO Series(BarHistory source, int period)
        {
            string key = CacheKey("VZO", period);
            if (source.Cache.ContainsKey(key))
                return (VZO)source.Cache[key];
            VZO v = new VZO(source, period);
            source.Cache[key] = v;
            return v;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 14);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period= Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            var R = new TimeSeries(DateTimes);
            var TV = new EMA(bars.Volume, period);
            if (R.Count > 0)
                R[0] = 0d;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar > 0)
                    R[bar] = Math.Sign(bars.Close[bar] - bars.Close[bar - 1]) * bars.Volume[bar];
            }
            var VP = new EMA(R, period);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (TV[bar] != 0)
                    Values[bar] = 100 * VP[bar] / TV[bar];
            }
            PrefillNan(period);
        }

        public override string Name => "VZO";

        public override string Abbreviation => "VZO";

        public override string HelpDescription => "The Volume Zone Oscillator by W.Khalil and D.Steckler takes into account both time and volume fluctuations from bearish to bullish and is designed to work in trending and non-trending conditions.";

        public override string PaneTag => @"VZO";

        public override WLColor DefaultColor => WLColor.DarkBlue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

    }    
}