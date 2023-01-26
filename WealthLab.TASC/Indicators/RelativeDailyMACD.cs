using WealthLab.Core;
using WealthLab.Indicators;
using System.Linq;

namespace WealthLab.TASC
{
    public class RelativeDailyMACD : IndicatorBase
    {
        //parameterless constructor
        public RelativeDailyMACD() : base()
        {
        }

        //for code based construction
        public RelativeDailyMACD(TimeSeries source, Int32 dailyLength1, Int32 dailyLength2, Int32 weeklyLength1, Int32 weeklyLength2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = dailyLength1;
            Parameters[2].Value = dailyLength2;
            Parameters[3].Value = weeklyLength1;
            Parameters[4].Value = weeklyLength2;

            Populate();
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Daily Length 1", ParameterType.Int32, 12);
            AddParameter("Daily Length 2", ParameterType.Int32, 26);
            AddParameter("Weekly Length 1", ParameterType.Int32, 60);
            AddParameter("Weekly Length 2", ParameterType.Int32, 130);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 DailyLength1 = Parameters[1].AsInt;
            Int32 DailyLength2 = Parameters[2].AsInt;
            Int32 WeeklyLength1 = Parameters[3].AsInt;
            Int32 WeeklyLength2 = Parameters[4].AsInt;

            DateTimes = ds.DateTimes;

            int period = new List<int>(){ DailyLength1, DailyLength2, WeeklyLength1, WeeklyLength2 }.Max();
            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(Math.Max(DailyLength1, DailyLength2), Math.Max(WeeklyLength1, WeeklyLength2));
            var WM = EMA.Series(ds, WeeklyLength1) - EMA.Series(ds, WeeklyLength2);
            var DM = EMA.Series(ds, DailyLength1) - EMA.Series(ds, DailyLength2);
            var RelativeDailyMACD = WM + DM;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = RelativeDailyMACD[bar];
            }
            PrefillNan(period);
        }

        public override string Name => "RelativeDailyMACD";

        public override string Abbreviation => "RelativeDailyMACD";

        public override string HelpDescription => "Relative Daily MACD by Vitali Apirine from the December 2017 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"WDMACDPane";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}