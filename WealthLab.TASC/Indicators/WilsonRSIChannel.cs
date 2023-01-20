using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //WilsonRSIChannel Indicator class
    public class WilsonRSIChannel : IndicatorBase
    {
        //parameterless constructor
        public WilsonRSIChannel() : base()
        {
        }

        //for code based construction
        public WilsonRSIChannel(TimeSeries source, Int32 rsiPeriod, Int32 smoothPeriod, Double cord)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = rsiPeriod;
            Parameters[2].Value = smoothPeriod;
            Parameters[3].Value = cord;

            Populate();
        }

        //static method
        public static WilsonRSIChannel Series(TimeSeries source, int rsiPeriod, int smoothPeriod, double cord)
        {
            string key = CacheKey("WilsonRSIChannel", rsiPeriod, smoothPeriod, cord);
            if (source.Cache.ContainsKey(key))
                return (WilsonRSIChannel)source.Cache[key];
            WilsonRSIChannel wrc = new WilsonRSIChannel(source, rsiPeriod, smoothPeriod, cord);
            source.Cache[key] = wrc;
            return wrc;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("RSI Period", ParameterType.Int32, 21);
            AddParameter("Smoothing Period", ParameterType.Int32, 1);
            AddParameter("Cord", ParameterType.Double, 30);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 rsiperiod = Parameters[1].AsInt;
            Int32 smoothperiod = Parameters[2].AsInt;
            Double cord = Parameters[3].AsDouble;

            DateTimes = ds.DateTimes;

            int period = Math.Max(rsiperiod, smoothperiod);
            if (period <= 0 || ds.Count == 0)
                return;

            //Remember parameters
            TimeSeries factor = (new RSI(ds, rsiperiod) - cord) / 100;
            var smooth = new EMA(ds - ds * factor, smoothperiod);

            //Assign first bar that contains indicator data
            var FirstValidValue = rsiperiod + smoothperiod;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = 0;

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                Values[bar] = smooth[bar];
            PrefillNan(FirstValidValue);
        }


        public override string Name => "WilsonRSIChannel";

        public override string Abbreviation => "WilsonRSIChannel";

        public override string HelpDescription => "Relative Price Channel, from the July 2006 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.BurlyWood;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}