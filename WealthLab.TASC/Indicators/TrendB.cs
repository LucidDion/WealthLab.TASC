using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class TrendB : IndicatorBase
    {
        bool CrossOver(int bar, TimeSeries ds1, TimeSeries ds2)
        {
            if (bar < 1) return false;
            return (ds1[bar] > ds2[bar] && ds1[bar - 1] <= ds2[bar - 1]);
        }

        bool CrossUnder(int bar, TimeSeries ds1, TimeSeries ds2)
        {
            if (bar < 1) return false;
            return (ds1[bar] < ds2[bar] && ds1[bar - 1] >= ds2[bar - 1]);
        }

        //parameterless constructor
        public TrendB() : base()
        {
        }
        
        //for code based construction
        public TrendB(TimeSeries source, Int32 period1, Int32 period2, Int32 m, Int32 n, Int32 c, bool UseRMSNoise)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = m;
            Parameters[4].Value = n;
            Parameters[5].Value = c;
            Parameters[6].Value = UseRMSNoise;

            Populate();
        }

        //static method
        public static TrendB Series(TimeSeries source, int period1, int period2, int m, int n, int c, bool UseRMSNoise)
        {
            string key = CacheKey("TrendB", period1, period2, m, n, c, UseRMSNoise);
            if (source.Cache.ContainsKey(key))
                return (TrendB)source.Cache[key];
            TrendB tb = new TrendB(source, period1, period2, m, n, c, UseRMSNoise);
            source.Cache[key] = tb;
            return tb;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period1", ParameterType.Int32, 10);
            AddParameter("Period2", ParameterType.Int32, 40);
            AddParameter("m", ParameterType.Int32, 8);
            AddParameter("n", ParameterType.Int32, 250);
            AddParameter("c", ParameterType.Int32, 4);
            AddParameter("UseRMSNoise", ParameterType.Boolean, true);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 m = Parameters[3].AsInt;
            Int32 n = Parameters[4].AsInt;
            Int32 c = Parameters[5].AsInt;
            Boolean useRMSNoise = Parameters[6].AsBoolean;

            DateTimes = ds.DateTimes;
            var period = new List<int> { period1, period2, m, n, c }.Max();

            if (period <= 0 || ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            if (n == 0)
                n = 1;
            n = Math.Abs(n);
            var FirstValidValue = n;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            if (ds.Count < n || ds == null)
                return;

            /* CPC and CPCTrend*/
            var lpf1 = new EMA(ds, period1);
            var lpf2 = new EMA(ds, period2);

            var hCPC = new TimeSeries(DateTimes);
            var hTrend = new TimeSeries(DateTimes);

            double expnt = 2d / (1 + m);
            double cumSum = 0d;            
            int xBar = 0;
            var mom = new Momentum(ds, 1);
            hCPC[0] = hTrend[0] = 0d;
            for (int bar = 0; bar < ds.Count; bar++)
            {
                hTrend[bar] = 0d;
            }

            for (int bar = 1; bar < ds.Count; bar++)
            {
                if (CrossOver(bar, lpf1, lpf2) || CrossUnder(bar, lpf1, lpf2))
                {
                    cumSum = 0d;
                    xBar = bar;
                }
                else
                    cumSum += mom[bar];

                hCPC[bar] = cumSum;

                /* Calculate the Trend with a piecewise EMA */
                if (bar - xBar > 0)
                {
                    double diff = expnt * (hCPC[bar] - hTrend[bar - 1]);
                    hTrend[bar] = hTrend[bar - 1] + diff;
                }
            }

            /* Trend-Noise Balance*/
            TimeSeries hDT = hCPC - hTrend;
            TimeSeries hNoise;

            if (useRMSNoise)
            {
                var hDTms = new SMA(hDT * hDT, n);
                hNoise = new TimeSeries(DateTimes);
                for (int bar = 0; bar < ds.Count; bar++)                
                    hNoise[bar] = Math.Sqrt(hDTms[bar]);                
            }
            else
            {
                hNoise = FastSMA.Series(hDT.Abs(), n);                
            }
            hNoise *= c;

            hNoise = hNoise.Abs();
            hTrend = hTrend.Abs();

            var hB = hTrend + hNoise;
            hB = 100 * hTrend / hB;

            for (int bar = 0; bar < ds.Count; bar++)
                Values[bar] = hB[bar];
            PrefillNan(period);
        }
        
        public override string Name => "TrendB";

        public override string Abbreviation => "TrendB";

        public override string HelpDescription => "Trend Noise Balance (B-Indicator) from the April 2004 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"TrendB";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;        
    }
}