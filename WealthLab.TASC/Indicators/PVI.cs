using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //PVI Indicator class
    public class PVI : IndicatorBase
    {
        //parameterless constructor
        public PVI() : base()
        {
        }

        //for code based construction
        public PVI(BarHistory source)
        : base()
        {
            Parameters[0].Value = source;

            Populate();
        }

        //static method
        public static PVI Series(BarHistory source)
        {
            string key = CacheKey("PVI");
            if (source.Cache.ContainsKey(key))
                return (PVI)source.Cache[key];
            PVI p = new PVI(source);
            source.Cache[key] = p;
            return p;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;

            DateTimes = bars.DateTimes;

            //Assign first bar that contains indicator data
            var FirstValidValue = 1;
            if (FirstValidValue > bars.Count) FirstValidValue = bars.Count;

            //Rest of series
            double Value = 0;
            for (int bar = FirstValidValue; bar < bars.Count; bar++)
            {
                if (bars.Volume[bar] > bars.Volume[bar - 1])
                    Value += 100 * bars.Close[bar] / bars.Close[bar - 1] - 100;
                Values[bar] = Value;
            }
        }

        public override string Name => "PVI";

        public override string Abbreviation => "PVI";

        public override string HelpDescription => "PVI (Positive Volume Index) from the April 2003 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "PVI";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}