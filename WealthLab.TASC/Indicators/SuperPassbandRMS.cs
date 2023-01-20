using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SuperPassbandRMS : IndicatorBase
    {
        //parameterless constructor
        public SuperPassbandRMS() : base()
        {
        }

        //for code based construction
        public SuperPassbandRMS(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static SuperPassbandRMS Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("SuperPassbandRMS", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (SuperPassbandRMS)source.Cache[key];
            SuperPassbandRMS spbr = new SuperPassbandRMS(source, period1, period2);
            source.Cache[key] = spbr;
            return spbr;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period1", ParameterType.Int32, 40);
            AddParameter("Period2", ParameterType.Int32, 60);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            var FirstValidValue = Math.Max(period1, period2);
            if (FirstValidValue > ds.Count || FirstValidValue < 0) FirstValidValue = ds.Count;

            if (FirstValidValue <= 0 || ds.Count == 0)
                return;

            if (ds.Count < Math.Max(49, FirstValidValue)) return;

            var spb = new SuperPassband(ds, period1, period2);

            var RMS = new TimeSeries(DateTimes);
            for (int bar = 0; bar < ds.Count; bar++)
                RMS[bar] = 0d;

            for (int bar = 49; bar < ds.Count; bar++)
            {
                for (int count = 0; count <= 49; count++)
                    RMS[bar] = RMS[bar] + spb[bar - count] * spb[bar - count];

                RMS[bar] = Math.Sqrt(RMS[bar] / 50d);
                Values[bar] = RMS[bar];
            }
        }

        public override string Name => "SuperPassbandRMS";

        public override string Abbreviation => "SuperPassbandRMS";

        public override string HelpDescription => "The root mean square (RMS) value of the cyclic output of the Super Passband filter by John Ehlers can be used as trigger points which can increase the efficiency of entries and exits.";

        public override string PaneTag => @"SuperPassband";

        public override WLColor DefaultColor => WLColor.Yellow;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}