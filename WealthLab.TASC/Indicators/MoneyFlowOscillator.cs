﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class MoneyFlowOscillator : IndicatorBase
    {
        //parameterless constructor
        public MoneyFlowOscillator() : base()
        {
        }

        //for code based construction
        public MoneyFlowOscillator(BarHistory source, Int32 period)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static MoneyFlowOscillator Series(BarHistory source, int period)
        {
            string key = CacheKey("MoneyFlowOscillator", period);
            if (source.Cache.ContainsKey(key))
                return (MoneyFlowOscillator)source.Cache[key];
            MoneyFlowOscillator mfo = new MoneyFlowOscillator(source, period);
            source.Cache[key] = mfo;
            return mfo;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 20);
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

            //DataSeries Multiplier = DataSeries.Abs((High - (Low>>1)) - ((High>>1)-Low)) /
            //	DataSeries.Abs((High - (Low>>1)) + ((High>>1)-Low));
            var Multiplier = ((bars.High - (bars.Low >> 1)) - ((bars.High >> 1) - bars.Low)) /
                ((bars.High - (bars.Low >> 1)) + ((bars.High >> 1) - bars.Low));
            var MFV = Multiplier * bars.Volume;
            var MFO = MFV.Sum(period) / bars.Volume.Sum(period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                Values[bar] = MFO[bar];
            }
        }
        
        public override string Name => "MoneyFlowOscillator";

        public override string Abbreviation => "MoneyFlowOscillator";

        public override string HelpDescription => "Created by Vitali Apirine (see article in October 2015 issue of Stocks and Commodities Magazine), the Money Flow Oscillator measures buying and selling pressure over a specific period of time.";

        public override string PaneTag => @"MoneyFlowOscillator";

        public override WLColor DefaultColor => WLColor.Black;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;
    }    
}