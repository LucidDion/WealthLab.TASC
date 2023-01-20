using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RevEngSMATC : IndicatorBase
    {
        //parameterless constructor
        public RevEngSMATC() : base()
        {
        }

        //for code based construction
        public RevEngSMATC(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static RevEngSMATC Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("RevEngSMATC", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (RevEngSMATC)source.Cache[key];
            RevEngSMATC res = new RevEngSMATC(source, period1, period2);
            source.Cache[key] = res;
            return res;
        }


        //name
        public override string Name
        {
            get
            {
                return "Reverse Engineered SMA Tomorrow's Close";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "RevEngSMATC";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "The RevEngSMATC indicator was derived by Tsokakis in the February 2007 issue of Stocks & Commodities "
                    + "magazine. 'TC' stands for 'tomorrow's close'. Tsokakis demonstrated that crossovers of this indicator "
                    + "with the source series occurred one bar earlier than crossovers of SMA indicators of the specified "
                    + "periods a very high percentage of the time.  \r\n\r\nFor example, create line plots of bars.Close (C) and "
                    + "RevEngSMATC(C, 20, 30). You can expect that C crosses under RevEngSMATC one bar before "
                    + "SMA(C, 20) crosses below SMA(C, 30) and vice-versa for cross over.";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "RevEngSMATC";
            }
        }

        //default color
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.Black;
            }
        }

        //default plot style
        public override PlotStyle DefaultPlotStyle
        {
            get
            {
                return PlotStyle.Line;
            }
        }

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = source.DateTimes;

            SMA sma1 = new SMA(source, period1 - 1);
            SMA sma2 = new SMA(source, period2 - 1);

            if (period1 < 1 || period1 > source.Count + 1) period1 = source.Count + 1;
            if (period2 < 1 || period2 > source.Count + 1) period2 = source.Count + 1;

            int  firstValidIndex = source.FirstValidIndex + Math.Max(period1, period2) - 2;

            if (period2 == period1) return;

            for (int n = firstValidIndex; n < source.Count; n++)
            {
                Values[n] = (sma2[n] * period1 * (period2 - 1) - sma1[n] * period2 * (period1 - 1)) / (period2 - period1);
            }
        }

        public static double Value(int idx, TimeSeries source, int period1, int period2)
        {
            if (period1 < 1 || period1 > source.Count + 1) period1 = source.Count + 1;
            if (period2 < 1 || period2 > source.Count + 1) period2 = source.Count + 1;

            return (SMA.Value(idx, source, period2) * period1 * (period2 - 1) 
                  - SMA.Value(idx, source, period1) * period2 * (period1 - 1)) 
                  / (period2 - period1);            
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("period1", ParameterType.Int32, 20);
            AddParameter("period2", ParameterType.Int32, 50);

        }
    }
}