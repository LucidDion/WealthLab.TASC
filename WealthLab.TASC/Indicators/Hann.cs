using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class Hann : IndicatorBase
    {
        public override string Name => "Hann filter";
        public override string Abbreviation => "Hann";
        public override string HelpDescription => "Hann Windowed Lowpass FIR Filter by Dr. John Ehlers from S&C April 2023 issue.";
        public override string PaneTag => "Price";
        public override WLColor DefaultColor => WLColor.Indigo;
        
        //it's a smoother
        public override bool IsSmoother => true;

        public Hann()
        {
        }
        public Hann(TimeSeries source, int period = 14, bool undersample = false, int undersampleBars = 5)
        {
            base.Parameters[0].Value = source;
			base.Parameters[1].Value = period;
            base.Parameters[2].Value = undersample;
            base.Parameters[3].Value = undersampleBars;
            this.Populate();
        }

        //static method
        public static Hann Series(TimeSeries source, int period = 14, bool undersample = false, int undersampleBars = 5)
        {
            string key = CacheKey("Hann", period, undersample, undersampleBars);
            if (source.Cache.ContainsKey(key))
                return (Hann)source.Cache[key];
            Hann h = new Hann(source, period, undersample, undersampleBars);
            source.Cache[key] = h;
            return h;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close); 
            base.AddParameter("Lookback Period", ParameterType.Int32, 14);
            base.AddParameter("Undersample", ParameterType.Boolean, false);
            base.AddParameter("Undersample bars", ParameterType.Int32, 5);
        }

        public override void Populate()
        {
            TimeSeries source = base.Parameters[0].AsTimeSeries;
            int period = base.Parameters[1].AsInt;
            bool undersample = base.Parameters[2].AsBoolean;
            int underSampleBars = base.Parameters[3].AsInt;

            this.DateTimes = source.DateTimes;
            int FirstValidValue = period;
            if (source.Count < FirstValidValue)
            {
                return;
            }

            if (undersample)
            {
                TimeSeries _ts = new TimeSeries(source.DateTimes, 0);
                for (int i = 0; i < _ts.Count; i++)
                {
                    if (i % underSampleBars == 0)
                        _ts[i] = source[i];
                    else
                        if (i > 0)
					_ts[i] = source[i - 1];
                }
                source = _ts;                
            }

            for (int bar = 0; bar < source.Count; bar++)
            {
                double Filt = 0, Coef = 0;

                if (bar > period)
                {
                    for (int count = 1; count <= period; count++)
                    {
                        double ang = 360 * count / (period + 1.0);
                        double c = 1.0 - Math.Cos(ang.ToRadians());
                        Filt += c * source[bar - count - 1];
                        Coef += c;
                    }
                }

                if ((Filt + Coef != 0))
                    base.Values[bar] = Filt / Coef;
            }
        }
    }
}