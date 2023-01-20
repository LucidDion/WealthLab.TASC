using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //Ehlers' FM Demodulator - May 2021
    public class FMDemodulator : IndicatorBase
    {
        public FMDemodulator()
        {
        }

        public FMDemodulator(BarHistory source, int period)
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Populate();
        }

        public static FMDemodulator Series(BarHistory source, int period)
        {
            string key = CacheKey("FMDemodulator", period);
            if (source.Cache.ContainsKey(key))
                return (FMDemodulator)source.Cache[key];
            FMDemodulator fm = new FMDemodulator(source, period);
            source.Cache[key] = fm;
            return fm;
        }

        public override string Name => "FM Demodulator";

        public override string Abbreviation => "FMDemodulator";

        public override string HelpDescription => "John Ehlers' FM (frequency modulation) Demodulator indicator from the May 2021 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "FMDemodulator";

        public override WLColor DefaultColor => WLColor.BlueViolet;

        public override void Populate()
        {
            //obtain parameter values
            BarHistory source = Parameters[0].AsBarHistory;
            int period = Parameters[1].AsInt;
            DateTimes = source.DateTimes;
            if (Count < period)
                return;

            //Derivative to establish zero mean (Basically the same as Close - Close[1], but removes intraday gap openings)
            TimeSeries Deriv = source.Close - source.Open;
            TimeSeries SS = new TimeSeries(source.DateTimes);

            for (int bar = 0; bar < 3; bar++)
            {
                SS[bar] = Deriv[bar];
            }

            //Hard limiter to remove AM noise
            TimeSeries HL = 10 * Deriv;
            for (int bar = period; bar < source.Count; bar++)
            {
                if( HL[bar] > 1) 
                    HL[bar] = 1;
                if (HL[bar] < -1)
                    HL[bar] = -1;
            }

            //super smooth coefficient
            double a1 = Math.Exp(-1.414 * Math.PI / period);
            double b1 = 1.414 * 180.0 / period;
            b1 = b1.ToRadians();
            b1 = 2.0 * a1 * Math.Cos(b1);
            double c2 = b1;
            double c3 = -a1 * a1;
            double c1 = 1.0 - c2 - c3;

            for (int bar = 3; bar < source.Count; bar++)
            {
                SS[bar] = c1 * (HL[bar] + HL[bar-1]) / 2d + c2 * SS[bar-1] + c3 * SS[bar-2];
            }

            Values = SS.Values;
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 30);
        }
    }
}