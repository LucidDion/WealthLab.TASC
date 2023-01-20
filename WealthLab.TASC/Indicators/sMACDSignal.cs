using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class sMACDSignal : IndicatorBase
    {
        //parameterless constructor
        public sMACDSignal() : base()
        {
        }

        //for code based construction
        public sMACDSignal(TimeSeries source, Int32 period1, Int32 period2)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;

            Populate();
        }

        //static method
        public static sMACDSignal Series(TimeSeries source, int period1, int period2)
        {
            string key = CacheKey("sMACDSignal", period1, period2);
            if (source.Cache.ContainsKey(key))
                return (sMACDSignal)source.Cache[key];
            sMACDSignal sms = new sMACDSignal(source, period1, period2);
            source.Cache[key] = sms;
            return sms;
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

            if (period <= 0 || ds.Count == 0)
                return;


            var FirstValidValue = Math.Max(period1, period2);

            if (FirstValidValue > ds.Count || FirstValidValue < 0)
                FirstValidValue = ds.Count;
            if (ds.Count < Math.Max(period1, period2))
                return;

            sMACD smacd = sMACD.Series(ds, period1, period2);
            FastSMA sma = FastSMA.Series(smacd, 9);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = sma[bar];
            }
        }

        public override string Name => "sMACDSignal";

        public override string Abbreviation => "sMACDSignal";

        public override string HelpDescription => "This is the Signal Line of the sMACD indicator.";

        public override string PaneTag => @"sMACD";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

    }
}