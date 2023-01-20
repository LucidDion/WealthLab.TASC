using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SuperPassband : IndicatorBase
    {
        //parameterless constructor
        public SuperPassband() : base()
        {
        }

        //for code based construction
        public SuperPassband(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static SuperPassband Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("SuperPassband", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (SuperPassband)source.Cache[key];
            SuperPassband spb = new SuperPassband(source, period1, period2);
            source.Cache[key] = spb;
            return spb;
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

            double a1 = 5 / (double)period1;
            double a2 = 5 / (double)period2;

            for (int bar = 0; bar < ds.Count; bar++)
            {
                if (bar >= 2)
                {
                Values[bar] = (a1 - a2) * ds[bar] + (a2 * (1 - a1) - a1 * (1 - a2)) * ds[bar - 1] +
                        ((1 - a1) + (1 - a2)) * Values[bar - 1] - (1 - a1) * (1 - a2) * Values[bar - 2];
                }
                else
                    Values[bar] = 0;
            }            
        }


        public override string Name => "SuperPassband";

        public override string Abbreviation => "SuperPassband";

        public override string HelpDescription => "Created by John Ehlers (see article in July 2016 issue of Stocks and Commodities Magazine), the Super Passband oscillator is a filter with near-zero lag.";

        public override string PaneTag => @"SuperPassband";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}