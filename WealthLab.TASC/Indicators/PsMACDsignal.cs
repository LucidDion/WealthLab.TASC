using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class PsMACDsignal : IndicatorBase
    {
        //parameterless constructor
        public PsMACDsignal() : base()
        {
        }

        //for code based construction
        public PsMACDsignal(TimeSeries source, Int32 period1, Int32 period2, Int32 period3)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period1;
            Parameters[2].Value = period2;
            Parameters[3].Value = period3;

            Populate();
        }

        //static method
        public static PsMACDsignal Series(TimeSeries source, int period1, int period2, int period3)
        {
            string key = CacheKey("PsMACDsignal", period1, period2, period3);
            if (source.Cache.ContainsKey(key))
                return (PsMACDsignal)source.Cache[key];
            PsMACDsignal pms = new PsMACDsignal(source, period1, period2, period3);
            source.Cache[key] = pms;
            return pms;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period 1", ParameterType.Int32, 12);
            AddParameter("Period 2", ParameterType.Int32, 26);
            AddParameter("Period 3", ParameterType.Int32, 9);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period1 = Parameters[1].AsInt;
            Int32 period2 = Parameters[2].AsInt;
            Int32 period3 = Parameters[3].AsInt;

            DateTimes = ds.DateTimes;
            var period = new List<int> { period1, period2, period3 }.Max();

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = period;
            if (FirstValidValue > ds.Count || FirstValidValue < 0)
                FirstValidValue = ds.Count;
            if (ds.Count < Math.Max(Math.Max(period1, period2), period3))
                return;
            if (period3 <= 1)
                return;

            sMACD smacd = sMACD.Series(ds, period1, period2);
            int XY = period1 * period2;
            int XZ = period1 * period3;
            int YZ = period2 * period3;
            int XYZ = period1 * period2 * period3;
            FastSMA fsma = FastSMA.Series(smacd, period3);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = (smacd[bar] * (XYZ - XY) -
                       XYZ * fsma[bar] -
                       ds[period1 + 1] * (YZ - period2) +
                       ds[period2 + 1] * (XZ - period1) +
                       XY * smacd[period3 + 1]) / (XZ - YZ - period1 + period2);
            }
        }

        public override string Name => "PsMACDsignal";

        public override string Abbreviation => "PsMACDsignal";

        public override string HelpDescription => "PsMACDsignal auxiliary";

        public override string PaneTag => @"PsMACDsignal";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }
}