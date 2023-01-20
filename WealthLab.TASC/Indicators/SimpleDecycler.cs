using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SimpleDecycler : IndicatorBase
    {
        //parameterless constructor
        public SimpleDecycler() : base()
        {
        }

        //for code based construction
        public SimpleDecycler(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static SimpleDecycler Series(TimeSeries source, int period)
        {
            string key = CacheKey("SimpleDecycler", period);
            if (source.Cache.ContainsKey(key))
                return (SimpleDecycler)source.Cache[key];
            SimpleDecycler sd = new SimpleDecycler(source, period);
            source.Cache[key] = sd;
            return sd;
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

            if (period > ds.Count) period = ds.Count;

            var HP = new TimeSeries(DateTimes);
            double Deg2Rad = Math.PI / 180.0;
            double cosInDegrees = Math.Cos((.707 * 360 / period) * Deg2Rad);
            double sinInDegrees = Math.Sin((.707 * 360 / period) * Deg2Rad);
            double alpha1 = (cosInDegrees + sinInDegrees - 1) / cosInDegrees;

            //Highpass filter
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= period + 2)
                    HP[bar] = (1 - alpha1 / 2) * (1 - alpha1 / 2) * (ds[bar] - 2 * ds[bar - 1] + ds[bar - 2]) + 2 * (1 - alpha1) * HP[bar - 1] - (1 - alpha1) * (1 - alpha1) * HP[bar - 2];
                else
                    HP[bar] = 0;
            }

            //Decycle is the difference between the input data and HP
            for (int bar = period + 2; bar < ds.Count; bar++)
            {
                Values[bar] = ds[bar] - HP[bar];
            }
        }



        public override string Name => "SimpleDecycler";

        public override string Abbreviation => "SimpleDecycler";

        public override string HelpDescription => @"Created by John Ehlers (see article in September 2015 issue of Stocks and Commodities Magazine), the Simple Decycler indicator designed to indicate trend with virtually zero lag.";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.DarkRed;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }
}