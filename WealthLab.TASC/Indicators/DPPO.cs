using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class DPPO : IndicatorBase
    {
        //constructors
        public DPPO() : base()
        {
        }
        public DPPO(TimeSeries source, int fastPeriod = 12, int slowPeriod = 26, int weeklySlowPeriod = 130) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastPeriod;
            Parameters[2].Value = slowPeriod;
            Parameters[3].Value = weeklySlowPeriod;
            Populate();
        }

        //static method
        public static DPPO Series(TimeSeries source, int fastPeriod = 12, int slowPeriod = 26, int weeklySlowPeriod = 130)
        {
            string key = CacheKey("DPPO", fastPeriod, slowPeriod, weeklySlowPeriod);
            if (source.Cache.ContainsKey(key))
                return (DPPO)source.Cache[key];
            DPPO dppo = new DPPO(source, fastPeriod, slowPeriod, weeklySlowPeriod);
            source.Cache[key] = dppo;
            return dppo;
        }

        //Name
        public override string Name
        {
            get
            {
                return "Daily PPO";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "DPPO";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "Daily Percentage Price Oscillator, based on the article by Vitali Apirine in the February 2018 issue of Stocks & Commodities magazine. " +
                    "DPPO is a momentum oscillator calculated as the difference of two EMAs and divided by the *weeklySlowPeriod* used for WPPO: " +
                    "100 x ((EMA(*fastPeriod*) - EMA(*slowPeriod*) / EMA(*weeklySlowPeriod*) ).";
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
                return WLColor.Silver;
            }
        }

        //default plot style
        public override PlotStyle DefaultPlotStyle
        {
            get
            {
                return PlotStyle.DashedLine;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Fast Period (D)", ParameterType.Int32, 12);
            AddParameter("Slow Period (D)", ParameterType.Int32, 26);
            AddParameter("Slow Period (W)", ParameterType.Int32, 130);
        }

        //populate
        public override void Populate()
        {
            //get parameter values
            TimeSeries source = Parameters[0].AsTimeSeries;
            int fastPeriod = Parameters[1].AsInt;
            int slowPeriod = Parameters[2].AsInt;
            int slowPeriodWeekly = Parameters[3].AsInt;
            DateTimes = source.DateTimes;

            //calculate
            EMA emaFast = new EMA(source, fastPeriod);
            EMA emaSlow = new EMA(source, slowPeriod);
            EMA emaSlowWeekly = new EMA(source, slowPeriodWeekly);
            TimeSeries result = ((emaFast - emaSlow) / emaSlowWeekly) * 100.0;
            Values = result.Values;
        }
    }
}