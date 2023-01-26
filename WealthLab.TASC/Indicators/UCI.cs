using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //UCI Indicator class
    public class UCI : IndicatorBase
    {
        //parameterless constructor
        public UCI() : base()
        {
        }

        //for code based construction
        public UCI(TimeSeries source, Int32 period, Int32 volaPperiod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = volaPperiod;

            Populate();
        }

        //static method
        public static UCI Series(TimeSeries source, int period, int volaPeriod)
        {
            string key = CacheKey("UCI", period, volaPeriod);
            if (source.Cache.ContainsKey(key))
                return (UCI)source.Cache[key];
            UCI u = new UCI(source, period, volaPeriod);
            source.Cache[key] = u;
            return u;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 25);
            AddParameter("VolaPeriod", ParameterType.Int32, 12);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Int32 volaperiod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            //Avoid exception errors
            if (period < 12) period = 12; // 3 is minimum for LinearReg, that uses 1/4 of period
            
            period = period / 2;
            var y = new EMA(ds, period / 2) 
                         / new EMA(ds, period);
            var lry = new LR(y, period / 2);
            var dvs = new DVS(ds, volaperiod);

            //Assign first bar that contains indicator data
            var FirstValidValue = Math.Max(dvs.FirstValidIndex, ds.FirstValidIndex + period + period / 2 - 2);

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
                if (dvs[bar] != 0)
                    Values[bar] = 10000 * (lry[bar] - 1) / dvs[bar];
                //else
                //    Values[bar] = 0;
        }

        public override string Name => "UCI";

        public override string Abbreviation => "UCI";

        public override string HelpDescription => @"Universal Cycle Index (UCI) from the May 2005 issue of Stocks & Commodities magazine.  by Stuart Belknap, PhD: ""The UCI is nothing more than a normalized Moving Average Converging/Diverging (MACD) indicator.""";

        public override string PaneTag => @"UCI";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}