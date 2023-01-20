using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class FIRSMA : IndicatorBase
    {
        public override string Name => "FIRSMA";
        public override string Abbreviation => "FIRSMA";
        public override string HelpDescription => "The FIR SMA indicator by Dr. John Ehlers from S&C September 2021 issue.";
        public override string PaneTag => "FIRSMA";
        public override WLColor DefaultColor => WLColor.DarkRed;

        //it's not a smoother
        public override bool IsSmoother => false;

        public FIRSMA()
        {
        }
        public FIRSMA(BarHistory bh, int period = 20)
        {
            base.Parameters[0].Value = bh;
            base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static FIRSMA Series(BarHistory source, int period = 20)
        {
            string key = CacheKey("FIRSMA", period);
            if (source.Cache.ContainsKey(key))
                return (FIRSMA)source.Cache[key];
            FIRSMA f = new FIRSMA(source, period);
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
                int coef = 0;

                if (bar > period)
                {
                    for (int count = 1; count <= period; count++)
                    {
                        Filt[bar] += Deriv[bar - count];
                        coef++;
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