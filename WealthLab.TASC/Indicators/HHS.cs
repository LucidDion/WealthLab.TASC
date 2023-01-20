using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class HHS : IndicatorBase
    {
        //parameterless constructor
        public HHS() : base()
        {
        }

        //for code based construction
        public HHS(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static HHS Series(BarHistory source, int period)
        {
            string key = CacheKey("HHS", period);
            if (source.Cache.ContainsKey(key))
                return (HHS)source.Cache[key];
            HHS h = new HHS(source, period);
            source.Cache[key] = h;
            return h;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Highest Period", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var HHH = new TimeSeries(DateTimes);

            var hh = new Highest(bars.Low, period);
            var ll = new Lowest(bars.Low, period);

            //var FirstValidValue = period * 3;
            //for (int bar = FirstValidValue; bar < bars.Count; bar++)
            for (int bar = period; bar < bars.Count; bar++)
            {
                HHH[bar] = bars.High[bar] > bars.High[bar - 1] ?
                    ((bars.High[bar] - ll[bar]) /
                    (hh[bar] - ll[bar])) : 0;
            }

            var ema = EMA.Series(HHH, period) * 100;

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = ema[bar];
            }
            PrefillNan(period + 2);
        }


        public override string Name => "HHS";

        public override string Abbreviation => "HHS";

        public override string HelpDescription => "Created by V. Apirine, the higher high stochastic (HHS) is part of the higher high lower low stochastic (HHLLS) which is a momentum indicator–based system that helps determine the direction of a trend.";

        public override string PaneTag => @"HHLLS";

        public override WLColor DefaultColor => WLColor.Green;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}