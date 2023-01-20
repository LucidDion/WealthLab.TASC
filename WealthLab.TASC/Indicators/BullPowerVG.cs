using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //BullPowerVG Indicator class
    public class BullPowerVG : IndicatorBase
    {
        //parameterless constructor
        public BullPowerVG() : base()
        {
        }

        //for code based construction
        public BullPowerVG(BarHistory bars, int period)
            : base()
        {
            Parameters[0].Value = bars;

            Populate();
        }
        
        //static method
        public static BullPowerVG Series(BarHistory source, int period)
        {
            string key = CacheKey("BullPowerVG", period);
            if (source.Cache.ContainsKey(key))
                return (BullPowerVG)source.Cache[key];
            BullPowerVG bpvg = new BullPowerVG(source, period);
            source.Cache[key] = bpvg;
            return bpvg;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
        }

        //populate
        public override void Populate()
        {
            BarHistory ds = Parameters[0].AsBarHistory;
            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Rest of series
            for (int bar = 1; bar < ds.Count; bar++)
            {
                double C = ds.Close[bar];
                double O = ds.Open[bar];
                double C1 = ds.Close[bar - 1];
                double H = ds.High[bar];
                double L = ds.Low[bar];
                double r1 = Math.Max(O - C1, H - L);
                double r2 = Math.Max(H - Math.Min(C1, O), C - L);
                if (C > O) /* white candle */ Values[bar] = r1;
                else if (C < O) /* black candle */ Values[bar] = r2;
                else if (H - C < C - L) /* doji, longer lower shadow */ Values[bar] = r1;
                else if (H - C > C - L) /* doji, longer upper shadow */ Values[bar] = r2;
                else if (C > C1) /* symmetrical doji, going up */ Values[bar] = r1;
                else /* symmetrical doji, going down or no change */ Values[bar] = r2;
            }
        }



        public override string Name => "BullPowerVG";

        public override string Abbreviation => "BullPowerVG";

        public override string HelpDescription => @"The BullPower component of the BBB indicator from the October 2003 issue of Stocks & Commodities magazine.";

        public override string PaneTag => @"BullPower";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;

    }
}