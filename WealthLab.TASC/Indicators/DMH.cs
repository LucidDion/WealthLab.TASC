using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class DMH : IndicatorBase
    {
        public override string Name => "Directional Movement using Hann windows";
        public override string Abbreviation => "DMH";
        public override string HelpDescription => "The DMH indicator by Dr. John Ehlers from S&C December 2021 issue.";
        public override string PaneTag => "DMH";
        public override WLColor DefaultColor => WLColor.DarkRed;

        //it's not a smoother
        public override bool IsSmoother => false;

        public DMH()
        {
        }
        public DMH(BarHistory bh, int period = 14)
        {
            base.Parameters[0].Value = bh;
			base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static DMH Series(BarHistory source, int period = 14)
        {
            string key = CacheKey("DMH", period);
            if (source.Cache.ContainsKey(key))
                return (DMH)source.Cache[key];
            DMH f = new DMH(source, period);
            source.Cache[key] = f;
            return f;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null); 
            base.AddParameter("Lookback Period", ParameterType.Int32, 14);
        }

        public override void Populate()
        {
            BarHistory bh = base.Parameters[0].AsBarHistory;
            int period = base.Parameters[1].AsInt;

            this.DateTimes = bh.DateTimes;
            int FirstValidValue = period;
            if (bh.Count < FirstValidValue)
            {
                return;
            }

            double SF = 1.0 / (double)period;
            double coef = 1.0 - Math.Cos(((360 * (double)period) / (period + 1)).ToRadians());

            TimeSeries UpperMove = bh.High - (bh.High >> 1);
            TimeSeries LowerMove = (bh.Low >> 1) - bh.Low;
            TimeSeries ema = new TimeSeries(bh.DateTimes, 0);
            TimeSeries DMSum = new TimeSeries(DateTimes, 0);

            for (int bar = 0; bar < bh.Count; bar++)
            {
                double PlusDM = 0, MinusDM = 0;

                if (UpperMove[bar] > LowerMove[bar] && UpperMove[bar] > 0)
                    PlusDM = UpperMove[bar];
                else if 
                    (LowerMove[bar] > UpperMove[bar] && LowerMove[bar] > 0 )
                    MinusDM = LowerMove[bar];

                if (bar > 0)
                    ema[bar] = SF * (PlusDM - MinusDM) + (1 - SF) * ema[bar - 1];

                //Smooth Directional Movements with Hann Windowed FIR filter
                if (bar > period)
                {
                    for (int count = 1; count <= period; count++)
                    {
                        double ang = 360 * count / (period + 1);
                        double c = 1 - Math.Cos(ang.ToRadians());
                        DMSum[bar] += (c * ema[bar - count - 1]);
                    }
                }

                if (coef != 0)
                    DMSum[bar] /= coef;

                base.Values[bar] = DMSum[bar];
            }

            PrefillNan(period + 1);
        }
    }
}