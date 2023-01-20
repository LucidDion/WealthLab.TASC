using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class STMACD : IndicatorBase
    {
        //parameterless constructor
        public STMACD() : base()
        {
        }

        //for code based construction
        public STMACD(BarHistory bars, Int32 period, Int32 emaPeriod1, Int32 emaPeriod2)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;
            Parameters[2].Value = emaPeriod1;
            Parameters[3].Value = emaPeriod2;

            Populate();
        }

        //static method
        public static STMACD Series(BarHistory source, int period, int emaPeriod1, int emaPeriod2)
        {
            string key = CacheKey("STMACD", period, emaPeriod1, emaPeriod2);
            if (source.Cache.ContainsKey(key))
                return (STMACD)source.Cache[key];
            STMACD sm = new STMACD(source, period, emaPeriod1, emaPeriod2);
            source.Cache[key] = sm;
            return sm;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 45);
            AddParameter("EMA Fast Period", ParameterType.Int32, 12);
            AddParameter("EMA Slow Period", ParameterType.Int32, 26);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Int32 emaPeriod1 = Parameters[2].AsInt;
            Int32 emaPeriod2 = Parameters[3].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            var FirstValidValue = Math.Max(emaPeriod1, Math.Max(emaPeriod2, period));
            if (FirstValidValue > bars.Count || FirstValidValue < 0) FirstValidValue = bars.Count;

            var HP = Highest.Series(bars.High, period);
            var LP = Lowest.Series(bars.Low, period);
            var MA12 = EMA.Series(bars.Close, emaPeriod1);
            var MA26 = EMA.Series(bars.Close, emaPeriod2);
            var ST12 = (MA12 - LP) / (HP - LP);
            var ST26 = (MA26 - LP) / (HP - LP);
            var STMACD = (ST12 - ST26) * 100;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                Values[bar] = STMACD[bar];
            }
        }

        public override string Name => "STMACD";

        public override string Abbreviation => "STMACD";

        public override string HelpDescription => "Created by Vitali Apirine, the STMACD combines the stochastic oscillator and MACD, letting you define oversold/overbought MACD levels.";

        public override string PaneTag => @"STMACD";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}