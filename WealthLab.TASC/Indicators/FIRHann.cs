using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class FIRHann : IndicatorBase
    {
        public override string Name => "FIRHann";
        public override string Abbreviation => "FIRHann";
        public override string HelpDescription => "The FIR Hann Window indicator by Dr. John Ehlers from S&C September 2021 issue.";
        public override string PaneTag => "FIRHann";
        public override WLColor DefaultColor => WLColor.DarkRed;

        //it's not a smoother
        public override bool IsSmoother => false;

        public FIRHann()
        {
        }
        public FIRHann(BarHistory bh, int period = 20)
        {
            base.Parameters[0].Value = bh;
            base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static FIRHann Series(BarHistory source, int period = 20)
        {
            string key = CacheKey("FIRHann", period);
            if (source.Cache.ContainsKey(key))
                return (FIRHann)source.Cache[key];
            FIRHann f = new FIRHann(source, period);
            source.Cache[key] = f;
            return f;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            base.AddParameter("Lookback Period", ParameterType.Int32, 20);
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

            //Derivative of the price wave
            TimeSeries Deriv = bh.Close - bh.Open;
            TimeSeries Filt = new TimeSeries(DateTimes, 0);
            //TimeSeries Roc = new TimeSeries(DateTimes, 0);

            double coef = 1.0 - Math.Cos(((360 * (double)period) / (period + 1)).ToRadians());

            for (int bar = 0; bar < bh.Count; bar++)
            {
                if (bar > period)
                {
                    for (int count = 1; count <= period; count++)
                    {
                        double ang = 360 * count / (period + 1);
                        double c = 1 - Math.Cos(ang.ToRadians());
                        Filt[bar] += (c * Deriv[bar - count - 1]);
                    }
                }

                if (coef != 0)
                    Filt[bar] /= coef;

                //if (bar > 0)
                //    Roc[bar] = (period / 6.28) * (Filt[bar] - Filt[bar - 1]);

                base.Values[bar] = Filt[bar];
            }
            PrefillNan(period + 1);
        }

        //public override bool IsPrivate => true;
    }
}