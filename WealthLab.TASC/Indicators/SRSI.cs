using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SRSI : IndicatorBase
    {
        //parameterless constructor
        public SRSI() : base()
        {
        }

        //for code based construction
        public SRSI(TimeSeries source, Int32 srsiPeriod, Int32 wmaPeriod)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = srsiPeriod;
            Parameters[2].Value = wmaPeriod;

            Populate();
        }

        //static method
        public static SRSI Series(TimeSeries source, int srsiPeriod, int wmaPeriod)
        {
            string key = CacheKey("SRSI", srsiPeriod, wmaPeriod);
            if (source.Cache.ContainsKey(key))
                return (SRSI)source.Cache[key];
            SRSI s = new SRSI(source, srsiPeriod, wmaPeriod);
            source.Cache[key] = s;
            return s;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("SRSI Period", ParameterType.Int32, 6);
            AddParameter("WilderMA Period", ParameterType.Int32, 14);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 srsiPeriod = Parameters[1].AsInt;
            Int32 wmaPeriod = Parameters[2].AsInt;

            DateTimes = ds.DateTimes;

            var FirstValidValue = Math.Max(srsiPeriod, wmaPeriod);

            if (FirstValidValue <= 0 || ds.Count == 0)
                return;

            EMA R1 = new EMA(ds, srsiPeriod);
            var R2 = new TimeSeries(DateTimes);
            var R3 = new TimeSeries(DateTimes);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                R2[bar] = ds[bar] > R1[bar] ? ds[bar] - R1[bar] : 0;
                R3[bar] = ds[bar] < R1[bar] ? R1[bar] - ds[bar] : 0;
            }

            var R4 = new WilderMA(R2, wmaPeriod);
            var R5 = new WilderMA(R3, wmaPeriod);

            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                Values[bar] = R5[bar] == 0 ? 100 : 100 - (100 / (1 + (R4[bar] / R5[bar])));
            }
        }


        public override string Name => "SRSI";

        public override string Abbreviation => "SRSI";

        public override string HelpDescription => "SRSI (slow relative strength index) by Vitali Apirine from April 2015 issue of Technical Analysis of Stocks & Commodities magazine is a momentum price oscillator that measures change in price movements relative to an exponential moving average (EMA), oscillating between 0 and 100.";

        public override string PaneTag => @"SRSI";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }    
}