using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class CorrelationTrend : IndicatorBase
    {
        public override string Name
        {
            get
            {
                return "CorrelationTrend";
            }
        }
        public override string Abbreviation
        {
            get
            {
                return "CorrelationTrend";
            }
        }
        public override string HelpDescription
        {
            get
            {
                return "The CorrelationTrend indicator by Dr. John Ehlers from S&C May 2020 issue could help to identify the onset of a trend or the switch to a cyclic mode.";
            }
        }
        public override string PaneTag
        {
            get
            {
                return "CorrelationTrend";
            }
        }
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DarkRed;
            }
        }

        //it's not a smoother
        public override bool IsSmoother => false;

        public CorrelationTrend()
        {
        }
        public CorrelationTrend(TimeSeries ds, int period = 20)
        {
            base.Parameters[0].Value = ds;
			base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static CorrelationTrend Series(TimeSeries source, int period = 20)
        {
            string key = CacheKey("CorrelationTrend", period);
            if (source.Cache.ContainsKey(key))
                return (CorrelationTrend)source.Cache[key];
            CorrelationTrend ct = new CorrelationTrend(source, period);
            source.Cache[key] = ct;
            return ct;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close); 
            base.AddParameter("Lookback Period", ParameterType.Int32, 20);
        }
        public override void Populate()
        {
            TimeSeries ds = base.Parameters[0].AsTimeSeries;
            int period = base.Parameters[1].AsInt;

            this.DateTimes = ds.DateTimes;
            int FirstValidValue = period;
            if (ds.Count < FirstValidValue)
            {
                return;
            }
			
            for (int bar = 0; bar < ds.Count; bar++)
            {
                double Sx = 0, Sy = 0, Sxx = 0, Sxy = 0, Syy = 0;

                for (int count = 0; count < period; count++)
                {
                    var X = (bar - count > -1) ? ds[bar - count] : ds[0];
                    var Y = -count;
                    Sx += X;
                    Sy += Y;
                    Sxx += (X * X);
                    Sxy += (X * Y);
                    Syy += (Y * Y);
                }

                if ((((period * Sxx) - (Sx * Sx)) > 0) & (((period * Syy) - (Sy * Sy)) > 0))
                {
                    base.Values[bar] = (period * Sxy - Sx * Sy) / Math.Sqrt((period * Sxx - Sx * Sx) * (period * Syy - Sy * Sy));
                }
            }
        }
    }
}