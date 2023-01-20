using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //TrueLow Indicator class
    public class TrueHigh : IndicatorBase
    {
        //parameterless constructor
        public TrueHigh() : base()
        {
        }

        //for code based construction
        public TrueHigh(BarHistory bars)
            : base()
        {
            Parameters[0].Value = bars;
            Populate();
        }

        //static method
        public static TrueHigh Series(BarHistory source)
        {
            string key = CacheKey("TrueHigh");
            if (source.Cache.ContainsKey(key))
                return (TrueHigh)source.Cache[key];
            TrueHigh th = new TrueHigh(source);
            source.Cache[key] = th;
            return th;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
        }

        //(BarHistory bars, BarHistory barsFirst, BarHistory barsSecond, int periodRegression, int periodRegressionMomentum,
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;

            DateTimes = ds.DateTimes;

            if (DateTimes.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = 1;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series with zeroes
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = Math.Max(ds.High[bar], ds.Close[bar - 1]);
            PrefillNan(1);
        }

        //This static method allows ad-hoc calculation of TrueLow (single calc mode)
        public static double Value(int bar, BarHistory ds)
        {
            return Math.Max(ds.High[bar], ds.Close[bar - 1]);
        }

        public override string Name => "TrueHigh";

        public override string Abbreviation => "TrueHigh";

        public override string HelpDescription => "TrueHigh is the maximum of: the current bar's High, and the previous bar's Close";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.DarkRed;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Dots;
    }
}