using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //InverseFisher Indicator class
    public class InverseFisher : IndicatorBase
    {
        //parameterless constructor
        public InverseFisher() : base()
        {
        }

        //for code based construction
        public InverseFisher(TimeSeries source)
            : base()
        {
            Parameters[0].Value = source;

            Populate();
        }

        //static method
        public static InverseFisher Series(TimeSeries source)
        {
            string key = CacheKey("InverseFisher");
            if (source.Cache.ContainsKey(key))
                return (InverseFisher)source.Cache[key];
            InverseFisher _if = new InverseFisher(source);
            source.Cache[key] = _if;
            return _if;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double e2y = Math.Exp(2 * ds[bar]);
                Values[bar] = (e2y - 1) / (e2y + 1);
            }
        }

        //This static method allows ad-hoc calculation of InverseFisher (single calc mode)
        public static double Value(int bar, TimeSeries ds)
        {
            double e2y = Math.Exp(2 * ds[bar]);
            return (e2y - 1) / (e2y + 1);
        }

        public override string Name => "InverseFisher";

        public override string Abbreviation => "InverseFisher";

        public override string HelpDescription => "InverseFisher Transform Indicator from the May 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"InverseFisher";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}