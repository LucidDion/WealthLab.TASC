using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class WDPPO : IndicatorBase
    {
        //constructors
        public WDPPO() : base()
        {
        }
        public WDPPO(TimeSeries source, int fastPeriod = 12, int slowPeriod = 26, int weeklyFastPeriod = 60, int weeklySlowPeriod = 130) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastPeriod;
            Parameters[2].Value = slowPeriod;
            Parameters[3].Value = weeklyFastPeriod;
            Parameters[4].Value = weeklySlowPeriod;
            Populate();
        }

        //static method
        public static WDPPO Series(TimeSeries source, int fastPeriod = 60, int slowPeriod = 130, int weeklyFastPeriod = 60, int weeklySlowPeriod = 130)
        {
            string key = CacheKey("WDPPO", fastPeriod, slowPeriod, weeklyFastPeriod, weeklySlowPeriod);
            if (source.Cache.ContainsKey(key))
                return (WDPPO)source.Cache[key];
            WDPPO w = new WDPPO(source, fastPeriod, slowPeriod, weeklyFastPeriod, weeklySlowPeriod);
            source.Cache[key] = w;
            return w;
        }

        //Name
        public override string Name
        {
            get
            {
                return "Relative Daily PPO";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "W+DPPO";
            }
        }

        //help description
        public override string HelpDescription
        {
            get
            {
                return "Relative Daily Percentage Price Oscillator (RDPPO), based on the article by Vitali Apirine in the February 2018 issue of Stocks & Commodities magazine. " +
                    "RDPPO is the result of the addition of DPPO + WPPO and is typically shown with a centerline. Signals are generated from RDPPO line " +
                    "crossovers, centerline daily and weekly crossovers, and divergences.";
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
                return WLColor.Green;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Fast Period (D)", ParameterType.Int32, 12);
            AddParameter("Slow Period (D)", ParameterType.Int32, 26);
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
            int fastPeriodWeekly = Parameters[3].AsInt;
            int slowPeriodWeekly = Parameters[4].AsInt;
            DateTimes = source.DateTimes;

            //calculate
            DPPO dppo = new DPPO(source, fastPeriod, slowPeriod, slowPeriodWeekly);
            WPPO wppo = new WPPO(source, fastPeriodWeekly, slowPeriodWeekly);
            TimeSeries result = dppo + wppo;
            Values = result.Values;
        }

        //companions
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("WPPO");
                return c;
            }
        }
    }
}