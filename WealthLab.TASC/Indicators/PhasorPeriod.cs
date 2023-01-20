using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class PhasorPeriod : IndicatorBase
    {
        public override string Name => "PhasorPeriod";
        public override string Abbreviation => "PhasorPeriod";
        public override string HelpDescription => "The Phasor Period indicator by Dr. John Ehlers from S&C October 2022 issue.";
        public override string PaneTag => "PhasorPeriod";
        public override WLColor DefaultColor => WLColor.Red;

        //it's not a smoother
        public override bool IsSmoother => false;

        public PhasorPeriod()
        {
        }
        public PhasorPeriod(TimeSeries ds, int period = 28)
        {
            base.Parameters[0].Value = ds;
            base.Parameters[1].Value = period;
            this.Populate();
        }

        //static method
        public static PhasorPeriod Series(TimeSeries source, int period = 28)
        {
            string key = CacheKey("PhasorPeriod", period);
            if (source.Cache.ContainsKey(key))
                return (PhasorPeriod)source.Cache[key];
            PhasorPeriod pp = new PhasorPeriod(source, period);
            source.Cache[key] = pp;
            return pp;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            base.AddParameter("Lookback Period", ParameterType.Int32, 28);
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

            TimeSeries Real = new TimeSeries(DateTimes);
            TimeSeries Imag = new TimeSeries(DateTimes);
            TimeSeries Angle = new TimeSeries(DateTimes);
            TimeSeries DeltaAngle = new TimeSeries(DateTimes);
            TimeSeries DerivedPeriod = new TimeSeries(DateTimes);            

            for (int idx = 0; idx < ds.Count; idx++)
            {
                Real[idx] = 0d; Imag[idx] = 0d; Angle[idx] = 0d;
            }

            for (int bar = 0; bar < ds.Count; bar++)
            {
                //Correlate with Cosine wave having a fixed period
                double Sx = 0, Sy = 0, Sxx = 0, Sxy = 0, Syy = 0;

                for (int count = 0; count < period; count++)
                {
                    var X = (bar - count > 0) ? ds[bar - count - 1] : ds[0];
                    var Y = Math.Cos(360 * (count - 1) / period) * (180 / Math.PI);
                    Sx += X;
                    Sy += Y;
                    Sxx += (X * X);
                    Sxy += (X * Y);
                    Syy += (Y * Y);
                }

                if ((((period * Sxx) - (Sx * Sx)) > 0) & (((period * Syy) - (Sy * Sy)) > 0))
                {
                    Real[bar] = (period * Sxy - Sx * Sy) / Math.Sqrt((period * Sxx - Sx * Sx) * (period * Syy - Sy * Sy));
                }

                //Correlate with a Negative Sine wave having a fixed period
                Sx = 0; Sy = 0; Sxx = 0; Sxy = 0; Syy = 0;

                for (int count = 0; count < period; count++)
                {
                    var X = (bar - count > 0) ? ds[bar - count - 1] : ds[0];
                    var Y = -Math.Sin(360 * (count - 1) / period) * (180 / Math.PI);
                    Sx += X;
                    Sy += Y;
                    Sxx += (X * X);
                    Sxy += (X * Y);
                    Syy += (Y * Y);
                }

                if ((period * Sxx - Sx * Sx > 0) && (period * Syy - Sy * Sy > 0))
                    Imag[bar] = (period * Sxy - Sx * Sy) / Math.Sqrt((period * Sxx - Sx * Sx) * (period * Syy - Sy * Sy));

                //Compute the angle as an arctangent function and resolve ambiguity
                if (Real[bar] != 0)
                    Angle[bar] = 90 - Math.Atan(Imag[bar] / Real[bar]) * (180 / Math.PI);
                if (Real[bar] < 0)
                    Angle[bar] = Angle[bar] - 180;

                //compensate for angle wraparound
                if (bar > 1)
                {
                    if ((Math.Abs(Angle[bar - 1]) - Math.Abs(Angle[bar] - 360)) < (Angle[bar] - Angle[bar - 1]) && Angle[bar] > 90 && Angle[bar - 1] < -90)
                        Angle[bar] = Angle[bar] - 360;
                    //angle cannot go backwards
                    if ((Angle[bar] < Angle[bar - 1]) && ((Angle[bar] > -135 && Angle[bar - 1] < 135) || (Angle[bar] < -90 && Angle[bar - 1] < -90)))
                        Angle[bar] = Angle[bar - 1];
                }

                //Frequency derived from rate-change of phase
                if (bar > 1)
                {
                    DeltaAngle[bar] = Angle[bar] - Angle[bar - 1];
                    if (DeltaAngle[bar] <= 0)
                        DeltaAngle[bar] = DeltaAngle[bar - 1];
                    if (DeltaAngle[bar] != 0)
                        DerivedPeriod[bar] = 360d / DeltaAngle[bar];
                    if (DerivedPeriod[bar] > 60)
                        DerivedPeriod[bar] = 60;
                }

                base.Values[bar] = DerivedPeriod[bar];
            }
            PrefillNan(period);
        }
    }
}