using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public enum WhichMA { EMA, SMA }

    public class MHLMA : IndicatorBase
    {
        //parameterless constructor
        public MHLMA() : base()
        {
        }

        //for code based construction
        public MHLMA(BarHistory source, Int32 periodHigh, Int32 periodMA, WhichMA choice)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = periodHigh;
            Parameters[2].Value = periodMA;
            Parameters[3].Value = choice;

            Populate();
        }

        //static method
        public static MHLMA Series(BarHistory source, int periodHigh, int periodMA, WhichMA choice)
        {
            string key = CacheKey("MHLMA", periodHigh, periodMA, choice);
            if (source.Cache.ContainsKey(key))
                return (MHLMA)source.Cache[key];
            MHLMA m = new MHLMA(source, periodHigh, periodMA, choice);
            source.Cache[key] = m;
            return m;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Highest/Lowest Period", ParameterType.Int32, 15);
            AddParameter("MA Period", ParameterType.Int32, 50);
            Parameter p = AddParameter("EMA or SMA?", ParameterType.StringChoice, "EMA");
            p.Choices.Add("EMA");
            p.Choices.Add("SMA");
            p.TypeName = "WhichMA";
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 highPeriod = Parameters[1].AsInt;
            Int32 movPeriod = Parameters[2].AsInt;
            WhichMA option = (WhichMA)Enum.Parse(typeof(WhichMA), Parameters[3].AsString);

            DateTimes = bars.DateTimes;

            var period = Math.Max(highPeriod, movPeriod);
            if (period <= 0 || bars.Count == 0)
                return;

            TimeSeries tempMHL = new TimeSeries(DateTimes, 0);
            Highest HH = new Highest(bars.High, highPeriod);
            Lowest LL = new Lowest(bars.Low, highPeriod);
            tempMHL = (HH + LL) / 2d;

            TimeSeries ema = EMA.Series(tempMHL, movPeriod);
            TimeSeries sma = FastSMA.Series(tempMHL, movPeriod);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = tempMHL[bar];
                Values[bar] = option == WhichMA.EMA? ema[bar] : sma[bar];
            }
        }

        public override string Name => "MHLMA";

        public override string Abbreviation => "MHLMA";

        public override string HelpDescription => "Created by V. Apirine, the MHL MA is a moving average applied to the middle of the high-low range.";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.Green;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}