using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class LLS : IndicatorBase
    {
        //parameterless constructor
        public LLS() : base()
        {
        }

        //for code based construction
        public LLS(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static LLS Series(BarHistory source, int period)
        {
            string key = CacheKey("LLS", period);
            if (source.Cache.ContainsKey(key))
                return (LLS)source.Cache[key];
            LLS l = new LLS(source, period);
            source.Cache[key] = l;
            return l;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Lowest Period", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var LLL = new TimeSeries(DateTimes);
            var hh = new Highest(bars.Low, period);
            var ll = new Lowest(bars.Low, period);

            //var FirstValidValue = period * 3;
            //for (int bar = FirstValidValue; bar < bars.Count; bar++)
            for (int bar = period; bar < bars.Count; bar++)
            {
                LLL[bar] = bars.Low[bar] < bars.Low[bar - 1] ?
                    ((hh[bar] - bars.Low[bar]) /
                    (hh[bar] - ll[bar])) : 0;
            }

            var ema = EMA.Series(LLL, period) * 100;

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = ema[bar];
            }
            PrefillNan(period + 2);
        }


        public override string Name => "LLS";

        public override string Abbreviation => "LLS";

        public override string HelpDescription => "Created by V. Apirine, the lower low stochastic (LLS) is part of the higher high lower low stochastic (HHLLS) which is a momentum indicator–based system that helps determine the direction of a trend.";

        public override string PaneTag => @"HHLLS";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}