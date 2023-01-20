using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RelVol : IndicatorBase
    {
        //parameterless constructor
        public RelVol() : base()
        {
        }

        //for code based construction
        public RelVol(BarHistory bars, Int32 period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static RelVol Series(BarHistory source, int period)
        {
            string key = CacheKey("RelVol", period);
            if (source.Cache.ContainsKey(key))
                return (RelVol)source.Cache[key];
            RelVol rv = new RelVol(source, period);
            source.Cache[key] = rv;
            return rv;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 60);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;            
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || DateTimes.Count == 0)
                return;

            FastSMA fsma = FastSMA.Series(bars.Volume, period);
            StdDev stdev = StdDev.Series(bars.Volume, period);

            var FirstValidValue = period;
            if (FirstValidValue > bars.Count || FirstValidValue < 0) FirstValidValue = bars.Count;

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar >= period)
                {
                    double av = fsma[bar];
                    double sd = stdev[bar];
                    double relVol = (bars.Volume[bar] - av) / sd;
                    Values[bar] = relVol;
                }
                else
                {
                    Values[bar] = 0d;
                }
            }
        }

        public override string Name => "RelVol";

        public override string Abbreviation => "RelVol";

        public override string HelpDescription => "Created by Melvin Dickover (see article in April 2014 issue of Stocks and Commodities Magazine), Relative Volume is an auxiliary indicator that finds spikes of volume above a predefined number of standard deviations of the average volume of the lookback period.";

        public override string PaneTag => @"RelVol";

        public override WLColor DefaultColor => WLColor.Red;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Histogram;
    }    
}