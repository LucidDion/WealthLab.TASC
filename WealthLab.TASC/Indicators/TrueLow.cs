using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //TrueLow Indicator class
    public class TrueLow : IndicatorBase
    {
        //parameterless constructor
        public TrueLow() : base()
        {
        }

        //for code based construction
        public TrueLow(BarHistory bars)
            : base()
        {
            Parameters[0].Value = bars;

            Populate();
        }

        //static method
        public static TrueLow Series(BarHistory source)
        {
            string key = CacheKey("TrueLow");
            if (source.Cache.ContainsKey(key))
                return (TrueLow)source.Cache[key];
            TrueLow tl = new TrueLow(source);
            source.Cache[key] = tl;
            return tl;
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
                Values[bar] = Math.Min(ds.Low[bar], ds.Close[bar - 1]);
            PrefillNan(1);
        }

        //This static method allows ad-hoc calculation of TrueLow (single calc mode)
        public static double Value(int bar, BarHistory ds)
        {
            return Math.Min(ds.Low[bar], ds.Close[bar - 1]);
        }
        
        public override string Name => "TrueLow";

        public override string Abbreviation => "TrueLow";

        public override string HelpDescription => "TrueLow is the minimum of: the current bar's Low, and the previous bar's Close";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Dots;
    }
}