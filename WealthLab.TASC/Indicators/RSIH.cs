using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RSIH : IndicatorBase
    {
        public override string Name => "RSI using Hann windows";
        public override string Abbreviation => "RSIH";
        public override string HelpDescription => "The RSIH indicator by Dr. John Ehlers from S&C January 2022 issue.";
        public override string PaneTag => "RSIH";
        public override WLColor DefaultColor => WLColor.Yellow;
        
        //it's not a smoother
        public override bool IsSmoother => false;

        public RSIH()
        {
        }
        public RSIH(TimeSeries source, int period = 14)
        {
            base.Parameters[0].Value = source;
			base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static RSIH Series(TimeSeries source, int period = 14)
        {
            string key = CacheKey("RSIH", period);
            if (source.Cache.ContainsKey(key))
                return (RSIH)source.Cache[key];
            RSIH r = new RSIH(source, period);
            source.Cache[key] = r;
            return r;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close); 
            base.AddParameter("Lookback Period", ParameterType.Int32, 14);
        }

        public override void Populate()
        {
            TimeSeries source = base.Parameters[0].AsTimeSeries;
            int period = base.Parameters[1].AsInt;

            this.DateTimes = source.DateTimes;
            int FirstValidValue = period;
            if (source.Count < FirstValidValue)
            {
                return;
            }

            for (int bar = 0; bar < source.Count; bar++)
            {
                double CU = 0, CD = 0;

                //RSIH - RSI with Hann Windowing
                if (bar > period)
                {
                    //Accumulate "Closes Up" and "Closes Down"
                    for (int count = 1; count <= period; count++)
                    {
                        double ang = 360 * count / (period + 1.0);
                        double c = 1.0 - Math.Cos(ang.ToRadians());
                        double diff = source[bar - count - 1] - source[bar - count];
                        double diff2 = source[bar - count] - source[bar - count - 1];

                        if (diff > 0)
                            CU += c * diff;
                        if (diff2 > 0)
                            CD += c * diff2;
                    }
                }

                if ((CU + CD != 0))
                    base.Values[bar] = -1.0 * ((CU - CD) / (CU + CD));
            }
        }
    }
}