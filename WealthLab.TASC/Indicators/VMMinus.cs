using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class VMMinus : IndicatorBase
    {
        //parameterless constructor
        public VMMinus() : base()
        {
        }

        //for code based construction
        public VMMinus(BarHistory bars, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static VMMinus Series(BarHistory source, int period)
        {
            string key = CacheKey("VMMinus", period);
            if (source.Cache.ContainsKey(key))
                return (VMMinus)source.Cache[key];
            VMMinus v = new VMMinus(source, period);
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
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            //Avoid exceptions
            if (period < 1 || period > bars.Count + 1) period = bars.Count + 1;

            var _tr = new TR(bars).Sum(period);
            var _vmMinus = (bars.Low - (bars.High >> 1)).Abs().Sum(period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = _vmMinus[bar] / _tr[bar];
            }
        }

        public override string Name => "VMMinus";

        public override string Abbreviation => "VMMinus";

        public override string HelpDescription => "Vortex Movement (VM-) from the January 2010 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"VM";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}