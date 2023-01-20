using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RevEngRSI : IndicatorBase
    {
        //parameterless constructor
        public RevEngRSI() : base()
        {
        }

        //for code based construction
        public RevEngRSI(TimeSeries source, Int32 period, Double rsival)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = rsival;

            Populate();
        }

        //static method
        public static RevEngRSI Series(TimeSeries source, int period1, double rsival)
        {
            string key = CacheKey("RevEngRSI", period1, rsival);
            if (source.Cache.ContainsKey(key))
                return (RevEngRSI)source.Cache[key];
            RevEngRSI rer = new RevEngRSI(source, period1, rsival);
            source.Cache[key] = rer;
            return rer;
        }

        //name
        public override string Name
        {
            get
            {
                return "Reverse Engineered RSI";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "RevEngRSI";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "From the article 'Reverse Engineering the RSI' in the June 2003 issue of Stocks & Commodities magazine. The RevEngRSI indicator returns the price value required for the RSI to move to the specified value on the following bar.";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //default color
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.OrangeRed;
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
            Int32 period = Parameters[1].AsInt;
            Double rsival = Parameters[2].AsDouble;

            DateTimes = source.DateTimes;

            //Prepare intermediate series
            TimeSeries UC = new TimeSeries(DateTimes);  // up change
            TimeSeries DC = new TimeSeries(DateTimes);  // down change	  

            for (int bar = 0; bar < source.Count; bar++)
            {
                UC[bar] = 0d;
                DC[bar] = 0d;
            }
            
            for (int bar = 1; bar < source.Count; bar++)
                if (source[bar] > source[bar - 1])
                    UC[bar] = source[bar] - source[bar - 1];
                else
                    DC[bar] = source[bar - 1] - source[bar];
            TimeSeries AUC = new EMA(UC, 2 * period - 1);
            TimeSeries ADC = new EMA(DC, 2 * period - 1);
            AUC.Abs();

            //Assign first bar that contains indicator data
            int FirstValidIdx = source.FirstValidIndex + 2 * period - 1;
            if (FirstValidIdx > source.Count)
                FirstValidIdx = source.Count;

            //modify the code below to implement your own indicator calculation
            for (int n = FirstValidIdx; n < source.Count; n++)
            {
                double value = (period - 1) * (ADC[n] * rsival / (100 - rsival) - AUC[n]);
                if (value < 0)
                    value *= (100 - rsival) / rsival;

                Values[n] = source[n] + value;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("period", ParameterType.Int32, 14);
            AddParameter("rsiVal", ParameterType.Double, 50);

        }
        
    }
}