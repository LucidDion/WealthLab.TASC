using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //CandleCode Indicator class
    public class CandleCode : IndicatorBase
    {
        //parameterless constructor
        public CandleCode() : base()
        {
        }

        //for code based construction
        public CandleCode(BarHistory bars, int period)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = period;
            Populate();
        }

        //static method
        public static CandleCode Series(BarHistory source, int period)
        {
            string key = CacheKey("CandleCode", period);
            if (source.Cache.ContainsKey(key))
                return (CandleCode)source.Cache[key];
            CandleCode cc = new CandleCode(source, period);
            source.Cache[key] = cc;
            return cc;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 20);
        }


        //Constructor
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            Int32 period = Parameters[1].AsInt;

            //Assign first bar that contains indicator data
            var FirstValidValue = period;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            var US = new TimeSeries(DateTimes, 0); 
            var BS = new TimeSeries(DateTimes, 0);
            var LS = new TimeSeries(DateTimes, 0);

            //Build aux series
            for (int bar = 0; bar < ds.Count; bar++)
                if (ds.Open[bar] > ds.Close[bar])
                {
                    US[bar] = ds.High [bar] - ds.Open [bar];
                    BS[bar] = ds.Open [bar] - ds.Close[bar];
                    LS[bar] = ds.Close[bar] - ds.Low  [bar];
                }
                else
                {
                    US[bar] = ds.High[bar] - ds.Close[bar];
                    BS[bar] = ds.Close[bar] - ds.Open [bar];
                    LS[bar] = ds.Open [bar] - ds.Low  [bar];
                }

            var U_Upper = BBUpper.Series(US, period, 0.5);
            var U_Lower = BBLower.Series(US, period, 0.5);
            var B_Upper = BBUpper.Series(BS, period, 0.5);
            var B_Lower = BBLower.Series(BS, period, 0.5);
            var L_Upper = BBUpper.Series(LS, period, 0.5);
            var L_Lower = BBLower.Series(LS, period, 0.5);

            //Initialize start of series with zeroes
            //for (int bar = 0; bar < FirstValidValue; bar++)
            //    Values[bar] = 0;

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                int Code = 32 + 8 + 2;
                if (US[bar] > U_Upper[bar]) Code += 16; else if (US[bar] < U_Lower[bar]) Code -= 16;
                if (BS[bar] > B_Upper[bar]) Code +=  4; else if (BS[bar] < B_Lower[bar]) Code -=  4;
                if (LS[bar] > L_Upper[bar]) Code +=  1; else if (LS[bar] < L_Lower[bar]) Code -=  1;
                if (ds.Open[bar] > ds.Close[bar]) Code = -Code;
                Values[bar] = Code;
            }
        }


        public override string Name => "CandleCode";

        public override string Abbreviation => "CandleCode";

        public override string HelpDescription => @"In the March, 2001 issue of Stocks & Commodities magazine, Viktor Likhovidov shares a method of coding candlesticks.";

        public override string PaneTag => @"CandleCode";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;

    }
}