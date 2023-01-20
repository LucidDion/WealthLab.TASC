using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //Ehlers' AM Detector - May 2021
    public class AMDetector : IndicatorBase
    {
        public AMDetector()
        {
        }

        public AMDetector(BarHistory source, int envelopePeriod, int smoothingPeriod)
        {
            Parameters[0].Value = source;
            Parameters[1].Value = envelopePeriod;
            Parameters[2].Value = smoothingPeriod;
            Populate();
        }

        public static AMDetector Series(BarHistory source, int envelopePeriod, int smoothingPeriod)
        {
            string key = CacheKey("AMDetector", envelopePeriod, smoothingPeriod);
            if (source.Cache.ContainsKey(key))
                return (AMDetector)source.Cache[key];
            AMDetector am = new AMDetector(source, envelopePeriod, smoothingPeriod);
            source.Cache[key] = am;
            return am;
        }

        public override string Name => "AM Detector";

        public override string Abbreviation => "AMDetector";

        public override string HelpDescription => "John Ehlers' AM (amplitude modulation) Detector indicator from the May 2021 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => "AMDetector";

        public override WLColor DefaultColor => WLColor.Violet;

        public override void Populate()
        {
            //obtain parameter values
            BarHistory source = Parameters[0].AsBarHistory;
            int envelopePeriod = Parameters[1].AsInt;
            int smoothingPeriod = Parameters[2].AsInt;
            DateTimes = source.DateTimes;
            if (Count < envelopePeriod || Count < smoothingPeriod)
                return;

            //Derivative to establish zero mean (Basically the same as Close - Close[1], but removes intraday gap openings)
            var Deriv = source.Close - source.Open;
            var Envel = Highest.Series(Deriv.Abs(), envelopePeriod);
            var Volatil = SMA.Series(Envel, smoothingPeriod);

            Values = Volatil.Values;
        }

        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Envelope Period", ParameterType.Int32, 4);
            AddParameter("Smoothing Period", ParameterType.Int32, 8);
        }
    }
}