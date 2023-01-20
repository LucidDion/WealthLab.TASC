using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //Truncated BandPass by Ehlers from July 2020
    public class TruncBandPass : IndicatorBase
    {
        //constructors
        public TruncBandPass() : base()
        {
        }

        //for code based construction
        public TruncBandPass(TimeSeries source, Int32 period, double bandwidth, int length)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = bandwidth;
            Parameters[3].Value = length;
            Populate();
        }

        //static method
        public static TruncBandPass Series(TimeSeries source, int period, double bandwidth, int length)
        {
            string key = CacheKey("TruncBandPass", period);
            if (source.Cache.ContainsKey(key))
                return (TruncBandPass)source.Cache[key];
            TruncBandPass tbp = new TruncBandPass(source, period, bandwidth, length);
            source.Cache[key] = tbp;
            return tbp;
        }

        //name
        public override string Name => "Truncated Bandpass";

        //abbreviation
        public override string Abbreviation => "TruncBandPass";

        //description
        public override string HelpDescription => "Truncated Bandpass filter by John Ehlers from the July 2020 issue.";

        //pane tag
        public override string PaneTag => "Bandpass";

        //color
        public override WLColor DefaultColor => WLColor.DarkGray;

        //plot style
        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            double bandWidth = Parameters[2].AsDouble;
            int length = Parameters[3].AsInt;
            DateTimes = ds.DateTimes;

            double Deg2Rad = Math.PI / 180.0;
            double F1 = Math.Cos((360d / (double)period) * Deg2Rad);
            double G1 = Math.Cos((bandWidth * 360 / (double)period) * Deg2Rad);
            double S1 = 1d / G1 - Math.Sqrt(1d / (G1 * G1) - 1);

            double[] trunc = new double[101];
            int start = Math.Max(period, length) - 1;
            for(int n = start; n < ds.Count; n++)
            {
                //stack the trunc array
                for (int count = 100; count >= 2; count--)
                    trunc[count] = trunc[count - 1];
                trunc[length + 2] = 0.0;
                trunc[length + 1] = 0.0;
                for (int count = length; count >= 1; count--)
                {
                    int idx = n - count + 1;
                    if (count >= 0 && count + 2 < trunc.Length && idx - 2 >= 0 && idx < ds.Count)
                        trunc[count] = 0.5 * (1.0 - S1) * (ds[idx] - ds[idx - 2]) + F1 * (1.0 + S1) * trunc[count + 1] - S1 * trunc[count + 2];
                }
                Values[n] = trunc[1];
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 20);
            AddParameter("Bandwidth", ParameterType.Double, 0.1);
            AddParameter("Length", ParameterType.Int32, 10);
        }
    }
}