using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class WPPO : IndicatorBase
    {
        //constructors
        public WPPO() : base()
        {
        }
        public WPPO(TimeSeries source, int fastPeriod = 60, int slowPeriod = 130) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastPeriod;
            Parameters[2].Value = slowPeriod;
            Populate();
        }

        //static method
        public static WPPO Series(TimeSeries source, int fastPeriod = 60, int slowPeriod = 130)
        {
            string key = CacheKey("WPPO", fastPeriod, slowPeriod);
            if (source.Cache.ContainsKey(key))
                return (WPPO)source.Cache[key];
            WPPO w = new WPPO(source, fastPeriod, slowPeriod);
            source.Cache[key] = w;
            return w;
        }

        //Name
        public override string Name
        {
            get
            {
                return "Weekly PPO";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "WPPO";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "Weekly Percentage Price Oscillator, based on the article by Vitali Apirine in the February 2018 issue of Stocks & Commodities magazine. " +
                    "WPPO is a momentum oscillator calculated as a percentage of the difference of 2 EMAs: 100 x ((EMA(*fastPeriod*) / EMA(*slowPeriod*) - 1).";
            }
        }

        //pane tag
        public override string PaneTag
        {
            get
            {
                return "W+DPPO";
            }
        }

        //default color
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DarkGreen;
            }
        }

        //thick line
        public override PlotStyle DefaultPlotStyle
        {
            get
            {
                return PlotStyle.ThickLine;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Fast Period (W)", ParameterType.Int32, 60);
            AddParameter("Slow Period (W)", ParameterType.Int32, 130);
        }

        //populate
        public override void Populate()
        {
            //get parameter values
            TimeSeries source = Parameters[0].AsTimeSeries;
            int fastPeriod = Parameters[1].AsInt;
            int slowPeriod = Parameters[2].AsInt;
            DateTimes = source.DateTimes;

            //calculate
            EMA emaFast = new EMA(source, fastPeriod);
            EMA emaSlow = new EMA(source, slowPeriod);
            TimeSeries result = ((emaFast - emaSlow) / emaSlow) * 100.0;
            Values = result.Values;
        }

        //companions
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("WDPPO");
                return c;
            }
        }
    }
}