using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{    
    // Alpha Indicator Class
    public class Alpha2 : IndicatorBase
    {        
        //parameterless constructor
        public Alpha2() : base()
        {
        }

        //for code based construction
        public Alpha2(TimeSeries source, Int32 stddevperiod, Int32 linearregperiod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = stddevperiod;
            Parameters[2].Value = linearregperiod;

            Populate();
        }

        //static method
        public static Alpha2 Series(TimeSeries source, Int32 stddevperiod, Int32 linearregperiod)
        {
            string key = CacheKey("Alpha2", stddevperiod, linearregperiod);
            if (source.Cache.ContainsKey(key))
                return (Alpha2)source.Cache[key];
            Alpha2 alpha = new Alpha2(source, stddevperiod, linearregperiod);
            source.Cache[key] = alpha;
            return alpha;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("StdDev Period", ParameterType.Int32, 7);
            AddParameter("LinReg Period", ParameterType.Int32, 3);
        }

        //name
        public override string Name => "Alpha2";

        //abbreviation
        public override string Abbreviation => "Alpha2";

        //description
        public override string HelpDescription => "Based on an article by Rick Martinelli, published in the June 2006 issue of Stocks and Commodities Magazine. The Alpha indicator is a measure of how likely tomorrow's price will be away from normal distributed prices.";

        //price pane
        public override string PaneTag => "Alpha2";

        //default color
        public override WLColor DefaultColor => WLColor.Green;

        //default plot style
        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;

        //Constructor
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 stddevperiod = Parameters[1].AsInt;
            Int32 linearregperiod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            //Remember parameters
            var k2 = (linearregperiod + 1) / 3.0;
            var k1 = linearregperiod / 2.0;
            //standard deviation of price changes
            var sd = new StdDev(new Momentum(ds, 1), stddevperiod);

            //Avoid exception errors
            if (linearregperiod < 3 || linearregperiod > ds.Count + 1) linearregperiod = ds.Count + 1;

            //Assign first bar that contains indicator data
            //FirstValidValue = ds.FirstValidValue + Math.Max(linearregperiod, sdper) - 1;

            //Initialize start of series with zeroes, and begin accumulating values
            var s2 = 3 * ds[0];
            var s1 = 2 * ds[0];
            for (int bar = 0; bar < linearregperiod - 1; bar++)
            {
                s2 = s2 + (2 * ds[bar] - s1) / k2;
                s1 = s1 + (ds[bar] - ds[0]) / k1;
            }

            s2 = s2 + (2 * ds[linearregperiod - 1] - s1) / k2;
            s1 = s1 + (ds[linearregperiod - 1] - ds[0]) / k1;
            double predict = ((linearregperiod + 1) * s2 - (linearregperiod + 2) * s1) / (linearregperiod - 1); 
            if (sd[linearregperiod - 1] > 0)
                Values[linearregperiod - 1] = (predict - ds[linearregperiod - 1]) / sd[linearregperiod - 1];

            //Average rest of series
            for (int bar = linearregperiod; bar < ds.Count; bar++)
            {
                s2 = s2 + (2 * ds[bar] - s1) / k2;
                s1 = s1 + (ds[bar] - ds[bar - linearregperiod]) / k1;
                predict = ((linearregperiod + 1) * s2 - (linearregperiod + 2) * s1) / (linearregperiod - 1);
                if (sd[bar] > 0)
                    Values[bar] = (predict - ds[bar]) / sd[bar];
            }
        }        
    }
}