using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class sMACD : IndicatorBase
    {
        //parameterless constructor
        public sMACD() : base()
        {
        }

        //for code based construction
        public sMACD(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static sMACD Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("sMACD", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (sMACD)source.Cache[key];
            sMACD sm = new sMACD(source, period1, period2);
            source.Cache[key] = sm;
            return sm;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Shorter SMA", ParameterType.Int32, 12);
            AddParameter("Longer SMA", ParameterType.Int32, 26);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;
            int period = Math.Max(period1, period2);
            var FirstValidValue = Math.Max(period1, period2);

            if (period <= 0 || ds.Count == 0)
                return;

            if (FirstValidValue > ds.Count || FirstValidValue < 0)
                FirstValidValue = ds.Count;
            if (ds.Count < FirstValidValue)
                return;

            FastSMA ema1 = FastSMA.Series(ds, period1);
            FastSMA ema2 = FastSMA.Series(ds, period2);

            for (int bar = 0; bar < ds.Count; bar++)
            {
                Values[bar] = ema1[bar] - ema2[bar];
            }
        }

        public override string Name => "sMACD";

        public override string Abbreviation => "sMACD";

        public override string HelpDescription => "sMACD by Johnny Dough is a MACD indicator based on simple moving averages.";

        public override string PaneTag => @"sMACD";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

    }
}