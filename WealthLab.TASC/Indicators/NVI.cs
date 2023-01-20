using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //NVI Indicator class
    public class NVI : IndicatorBase
    {
        //parameterless constructor
        public NVI() : base()
        {
        }

        //for code based construction
        public NVI(BarHistory source)
        : base()
        {
            Parameters[0].Value = source;

            Populate();
        }

        //static method
        public static NVI Series(BarHistory source)
        {
            string key = CacheKey("NVI");
            if (source.Cache.ContainsKey(key))
                return (NVI)source.Cache[key];
            NVI nvi = new NVI(source);
            source.Cache[key] = nvi;
            return nvi;
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
                if (bars.Volume[bar] <= bars.Volume[bar - 1])
                    Value += 100 * bars.Close[bar] / bars.Close[bar - 1] - 100;
                Values[bar] = Value;
            }
        }

        public override string Name => "NVI";

        public override string Abbreviation => "NVI";

        public override string HelpDescription => "NVI (Negative Volume Index) from the April 2003 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "NVI";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}