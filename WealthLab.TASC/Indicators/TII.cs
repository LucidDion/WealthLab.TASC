using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class TII : IndicatorBase
    {
        //parameterless constructor
        public TII() : base()
        {
            OverboughtLevel = 80;
            OversoldLevel = 20;
        }

        //for code based construction
        public TII(TimeSeries source, int period, int maPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = maPeriod;
            OverboughtLevel = 80;
            OversoldLevel = 20;
            Populate();
        }

        //static method
        public static TII Series(TimeSeries source, int period, int maPeriod)
        {
            string key = CacheKey("TII", period, maPeriod);
            if (source.Cache.ContainsKey(key))
                return (TII)source.Cache[key];
            TII tii = new TII(source, period, maPeriod);
            source.Cache[key] = tii;
            return tii;
        }

        //Name
        public override string Name => "Trend Intensity Index";

        //Abbreviation
        public override string Abbreviation => "TII";

        //description
        public override string HelpDescription => "Trend Intensity Index (TII) by M.H. Pee from article in the June 2002 issue of Stocks and Commodities Magazine is used to indicate the strength of a current trend in the market.";

        //pane tag
        public override string PaneTag => "TII";

        //color
        public override WLColor DefaultColor => WLColor.Blue;

        //populate
        public override void Populate()
        {
            TimeSeries source = Parameters[0].AsTimeSeries;
            int period = Parameters[1].AsInt;
            int maPeriod = Parameters[2].AsInt;
            DateTimes = source.DateTimes;

            var FirstValidValue = Math.Max(period, maPeriod);

            /* If price is above the moving average, a positive deviation is recorded, 
            and if price is below the moving average a negative deviation. 
            The deviation is simply the distance between price and the moving average.

            Once the deviations are calculated, TII is calculated as:
            ( Sum of Positive Dev ) / ( ( Sum of Positive Dev ) + ( Sum of Negative Dev ) ) * 100 */

            TimeSeries pos = new TimeSeries(DateTimes);
            TimeSeries neg = new TimeSeries(DateTimes);            
            TimeSeries ma = FastSMA.Series(source, maPeriod);

            for (int i = FirstValidValue; i < source.Count; i++)
            {
                double p_diff = source[i] - ma[i];
                double n_diff = ma[i] - source[i];
                pos[i] = (p_diff > 0) ? p_diff : 0;
                neg[i] = (n_diff > 0) ? n_diff : 0;
            }

            TimeSeries SDPos = pos.Sum(period);
            TimeSeries SDNeg = neg.Sum(period);

            for (int bar = FirstValidValue; bar < source.Count; bar++)
            {
                Values[bar] = SDPos[bar] / (SDPos[bar] + SDNeg[bar]) * 100d;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 30);
            AddParameter("MA Period", ParameterType.Int32, 60);
        }
    }
}