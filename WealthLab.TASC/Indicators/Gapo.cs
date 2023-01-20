using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //Gapo Indicator class
    public class Gapo : IndicatorBase
    {
        //parameterless constructor
        public Gapo() : base()
        {
        }

        //for code based construction
        public Gapo(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static Gapo Series(BarHistory source, int period)
        {
            string key = CacheKey("Gapo", period);
            if (source.Cache.ContainsKey(key))
                return (Gapo)source.Cache[key];
            Gapo g = new Gapo(source, period);
            source.Cache[key] = g;
            return g;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Remember parameters
            var range = new Highest(ds.High, period) - new Lowest(ds.Low, period);
            var logperiod = Math.Log10(period);

            //Assign first bar that contains indicator data
            var FirstValidValue = range.FirstValidIndex + period;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = Math.Log10(range[bar]) / logperiod;
        }

        //This static method allows ad-hoc calculation of RSS (single calc mode)
        public static double Value(int bar, BarHistory ds, int period)
        {
            //Avoid exception errors
            if (period < 2) return Double.NaN;

            double range = Highest.Value(bar, ds.High, period) - Lowest.Value(bar, ds.Low, period);
            if (range <= 0) return Double.NaN;

            return Math.Log10(range) / Math.Log10(period);
        }

        public override string Name => "Gapo";

        public override string Abbreviation => "Gapo";

        public override string HelpDescription => "Gopalakrishnan Range Index, from the January 2000 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"Gapo";

        public override WLColor DefaultColor => WLColor.CadetBlue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}