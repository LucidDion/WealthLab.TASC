using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SZO : IndicatorBase
    {
        //parameterless constructor
        public SZO() : base()
        {
            OverboughtLevel = 7;
            OversoldLevel = -7;
        }

        //for code based construction
        public SZO(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            OverboughtLevel = 7;
            OversoldLevel = -7;
            Populate();
        }

        //static method
        public static SZO Series(TimeSeries source, int period)
        {
            string key = CacheKey("SZO", period);
            if (source.Cache.ContainsKey(key))
                return (SZO)source.Cache[key];
            SZO szo = new SZO(source, period);
            source.Cache[key] = szo;
            return szo;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = period * 3;

            if (ds == null || ds.Count == 0 || ds.Count < FirstValidValue) return;

            var R = new TimeSeries(DateTimes);
            var SP = new TimeSeries(DateTimes);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < period)
                    R[bar] = 0d;
                else
                    R[bar] = (ds[bar] > ds[bar - 1]) ? 1 : -1;
            }

            var sp = new TEMA_TASC(R, period);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= FirstValidValue)
                    Values[bar] = 100 * (sp[bar] / period);
                else
                    Values[bar] = 0d;
            }
        }

        public override string Name => "SZO";

        public override string Abbreviation => "SZO";

        public override string HelpDescription => "The Sentiment Zone Oscillator by W.Khalil measures extreme bearishness and bullishness to help identify a change in sentiment.";

        public override string PaneTag => @"SZO";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}