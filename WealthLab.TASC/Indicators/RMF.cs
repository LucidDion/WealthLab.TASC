using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //John Ehlers Recursive Median Filter indicator from TASC 2018-03
    public class RMF : IndicatorBase
    {
        //parameterless constructor
        public RMF() : base()
        {
        }

        //for code based construction
        public RMF(TimeSeries source, int period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Populate();
        }

        //static method
        public static RMF Series(TimeSeries source, int period)
        {
            string key = CacheKey("RMF", period);
            if (source.Cache.ContainsKey(key))
                return (RMF)source.Cache[key];
            RMF r = new RMF(source, period);
            source.Cache[key] = r;
            return r;
        }


        //Name
        public override string Name
        {
            get
            {
                return "Recursive Median Filter";
            }
        }

        //Abbreviation
        public override string Abbreviation
        {
            get
            {
                return "RMF";
            }
        }

        //Help description
        public override string HelpDescription
        {
            get
            {
                return "John Ehlers' Recursive Median Filter indicator from the March 2018 issue of Technical Analysis of Stocks & Commodities magazine.";
            }
        }

        //Plot on source pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //color
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DodgerBlue;
            }
        }

        //it's a smoother
        public override bool IsSmoother
        {
            get
            {
                return true;
            }
        }

        //populate values
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            DateTimes = source.DateTimes;

            //set EMA constant from period
            double rad = 360.0 / period;
            rad = rad.ToRadians();
            double alpha1 = (Math.Cos(rad) + Math.Sin(rad) - 1.0) / Math.Cos(rad);

            //calculate indicator values, prime with median
            int start = period + source.FirstValidIndex;
            if (start >= source.Count)
                return;
            double rm = Median.Value(start - 1, source, period);
            Values[start - 1] = rm;
            for(int n = start; n < source.Count; n++)
            {
                rm = alpha1 * Median.Value(n, source, period) + (1.0 - alpha1) * rm;
                Values[n] = rm;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 12);
        }
    }
}