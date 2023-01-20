using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class FIRTriangle : IndicatorBase
    {
        public override string Name => "FIRTriangle";
        public override string Abbreviation => "FIRTriangle";
        public override string HelpDescription => "The FIR Triangle Window indicator by Dr. John Ehlers from S&C September 2021 issue.";
        public override string PaneTag => "FIRTriangle";
        public override WLColor DefaultColor => WLColor.DarkRed;

        //it's not a smoother
        public override bool IsSmoother => false;

        public FIRTriangle()
        {
        }
        public FIRTriangle(BarHistory bh, int period = 20)
        {
            base.Parameters[0].Value = bh;
            base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static FIRTriangle Series(BarHistory source, int period = 20)
        {
            string key = CacheKey("FIRTriangle", period);
            if (source.Cache.ContainsKey(key))
                return (FIRTriangle)source.Cache[key];
            FIRTriangle f = new FIRTriangle(source, period);
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

            for (int bar = 0; bar < bh.Count; bar++)
            {
                double SumCoef = 0, coef = 0;

                if (bar > period)
                {
                    for (int count = 1; count <= period; count++)
                    {
                        if (count < (period / 2))
                            coef = count;
                        if (count == (period / 2))
                            coef = period / 2d;
                        if (count > (period / 2))
                            coef = (period + 1 - count);

                        Filt[bar] += (coef * Deriv[bar - count - 1]);
                        SumCoef += coef;
                    }
                }

                if (SumCoef != 0)
                    Filt[bar] /= SumCoef;

                //if (bar > 0)
                //    Roc[bar] = (period / 6.28) * (Filt[bar] - Filt[bar - 1]);

                base.Values[bar] = Filt[bar];
            }
            PrefillNan(period + 1);
        }

        //public override bool IsPrivate => true;
    }
}