using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SVEHLZigZagTicks : IndicatorBase
    {
        public override string Name => "SVEHLZigZagTicks";

        public override string Abbreviation => "SVEHLZigZagTicks";

        public override string HelpDescription => "Sylvain Vervoort's SVEHLZigZagTicks indicator from September 2018 issue of Stocks & Commodities magazine";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;

        //parameterless constructor
        public SVEHLZigZagTicks() : base()
        {
        }

        //for code based construction
        public SVEHLZigZagTicks(BarHistory bars, int change)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = change;

            Populate();
        }

        //static method
        public static SVEHLZigZagTicks Series(BarHistory source, int change)
        {
            string key = CacheKey("SVEHLZigZagTicks", change);
            if (source.Cache.ContainsKey(key))
                return (SVEHLZigZagTicks)source.Cache[key];
            SVEHLZigZagTicks sve = new SVEHLZigZagTicks(source, change);
            source.Cache[key] = sve;
            return sve;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Number of Ticks", ParameterType.Int32, 200);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 change = Parameters[1].AsInt;
            var ticks = change;// * bars.TickSize;
            DateTimes = bars.DateTimes;

            if (bars.Count == 0)
                return;

            int CurrentTrend = 0;
            double Reverse = 0;
            double HPrice = 0;
            double LPrice = 0;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (CurrentTrend >= 0)   // trend is up, look for new swing high
                {
                    HPrice = Math.Max(bars.High[bar], HPrice);
                    Reverse = HPrice - ticks;
                    if (bars.Low[bar] <= Reverse)
                    {
                        CurrentTrend = -1;
                        LPrice = bars.Low[bar];
                        Reverse = LPrice + ticks;
                    }
                }
                if (CurrentTrend <= 0)   // trend is down, look for new swing low
                {
                    LPrice = Math.Min(bars.Low[bar], LPrice);
                    Reverse = LPrice + ticks;
                    if (bars.High[bar] >= Reverse)
                    {
                        CurrentTrend = 1;
                        HPrice = bars.High[bar];
                        Reverse = HPrice - ticks;
                    }
                }

                Values[bar] = Reverse;
            }
        }
    }
}