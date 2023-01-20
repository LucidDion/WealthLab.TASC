using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class FDSO : IndicatorBase
    {
        //parameterless constructor
        public FDSO() : base()
        {
        }

        //for code based construction
        public FDSO(TimeSeries source, Int32 period)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static FDSO Series(TimeSeries source, int period)
        {
            string key = CacheKey("FDSO", period);
            if (source.Cache.ContainsKey(key))
                return (FDSO)source.Cache[key];
            FDSO fdso = new FDSO(source, period);
            source.Cache[key] = fdso;
            return fdso;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 40);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = Math.Max(2, period);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            var Filt = new TimeSeries(DateTimes);

            //Smooth with a Super Smoother
            double Deg2Rad = Math.PI / 180.0;
            double a1 = Math.Exp(-1.414 * Math.PI / (0.5 * period));
            double b1 = 2.0 * a1 * Math.Cos((1.414 * 180d / (0.5 * period)) * Deg2Rad);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1 - c2 - c3;

            //Produce Nominal zero mean with zeros in the transfer response 
            //at DC and Nyquist with no spectral distortion
            //Nominally whitens the spectrum because of 6 dB per octave rolloff
            var Zeros = ds - (ds >> 2);

            var ScaledFilt = 0.0;
            var FisherFilt = 0.0;

            //Filt += 0;

            //if (FirstValidValue > 1)
            //    Filt[FirstValidValue - 1] = 0d;

            //SuperSmoother Filter
            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar < FirstValidValue)
                {
                    Filt[bar] = 0d; Values[bar] = 0d;
                }
                else
                {
                    Filt[bar] = c1 * (Zeros[bar] + Zeros[bar - 1]) / 2d + c2 * Filt[bar - 1] + c3 * Filt[bar - 2];

                    //Compute Standard Deviation
                    double RMS = 0;
                    for (int count = 0; count < period - 1; count++)
                    {
                        if (bar > period)
                            RMS += Math.Pow(Filt[bar - count], 2);
                    }
                    RMS = Math.Sqrt(RMS / (double)period);

                    //Rescale Filt in terms of Standard Deviations
                    if (RMS != 0)
                        ScaledFilt = Filt[bar] / RMS;

                    //Apply Fisher Transform to establish real Gaussian Probability Distribution
                    if (Math.Abs(ScaledFilt) < 2)
                        FisherFilt = 0.5 * Math.Log10((1 + ScaledFilt / 2d) / (1 - ScaledFilt / 2d));

                    if (bar > period)
                        Values[bar] = FisherFilt;
                    else
                        Values[bar] = 0;
                }
            }
            PrefillNan(period + 10);
        }

        public override string Name => "FDSO";

        public override string Abbreviation => "FDSO";

        public override string HelpDescription => "Created by John Ehlers, the FDSO (Fisherized Deviation Scaled Oscillator) is an oscillator that can be used in swing trading.";

        public override string PaneTag => @"DSO";

        public override WLColor DefaultColor => WLColor.DarkViolet;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}