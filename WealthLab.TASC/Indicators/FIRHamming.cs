using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class FIRHamming : IndicatorBase
    {
        public override string Name => "FIRHamming";
        public override string Abbreviation => "FIRHamming";
        public override string HelpDescription => "The FIR Hamming Window indicator by Dr. John Ehlers from S&C September 2021 issue.";
        public override string PaneTag => "FIRHamming";
        public override WLColor DefaultColor => WLColor.DarkRed;

        //it's not a smoother
        public override bool IsSmoother => false;

        public FIRHamming()
        {
        }
        public FIRHamming(BarHistory bh, int period = 20, int pedestal = 10)
        {
            base.Parameters[0].Value = bh;
            base.Parameters[1].Value = period;
            base.Parameters[2].Value = pedestal;
            this.Populate();
        }

        //static method
        public static FIRHamming Series(BarHistory source, int period = 20, int pedestal = 10)
        {
            string key = CacheKey("FIRHamming", period, pedestal);
            if (source.Cache.ContainsKey(key))
                return (FIRHamming)source.Cache[key];
            FIRHamming f = new FIRHamming(source, period, pedestal);
            source.Cache[key] = f;
            return f;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            base.AddParameter("Lookback Period", ParameterType.Int32, 20);
            base.AddParameter("Pedestal", ParameterType.Int32, 10);
        }

        public override void Populate()
        {
            BarHistory bh = base.Parameters[0].AsBarHistory;
            int period = base.Parameters[1].AsInt;
            int pedestal = base.Parameters[2].AsInt;

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

            double coef = Math.Sin((pedestal + (180 - 2.0 * pedestal) * period / (period - 1)).ToRadians());

            for (int bar = 0; bar < bh.Count; bar++)
            {
                if (bar > period)
                {
                    for (int count = 1; count < period; count++)
                    {
                        double ang = (pedestal + (180 - 2.0 * pedestal) * count / (period - 1));
                        double c = Math.Sin(ang.ToRadians());
                        Filt[bar] += (c * Deriv[bar - count]);
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