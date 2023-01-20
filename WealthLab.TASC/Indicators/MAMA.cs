﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class MAMA : IndicatorBase
    {
        //constructors
        public MAMA() : base()
        {
        }
        public MAMA(TimeSeries source, double fastLimit = 0.5, double slowLimit = 0.05) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = fastLimit;
            Parameters[2].Value = slowLimit;
            Populate();
        }

        //static method
        public static MAMA Series(TimeSeries source, double fastLimit = 0.5, double slowLimit = 0.05)
        {
            string key = CacheKey("MAMA", fastLimit, slowLimit);
            if (source.Cache.ContainsKey(key))
                return (MAMA)source.Cache[key];
            MAMA m = new MAMA(source, fastLimit, slowLimit);
            source.Cache[key] = m;
            return m;
        }


        //name
        public override string Name => "MESA Adaptive Moving Average";

        //abbreviation
        public override string Abbreviation => "MAMA";

        //description
        public override string HelpDescription => "MESA Adaptive Moving Average by John Ehlers from the September 2001 issue of Technical Analysis of Stocks & Commodities magazine";

        //plot in price pane
        public override string PaneTag => "Price";

        //default color
        public override WLColor DefaultColor => WLColor.Red;

        //it's a smoother
        public override bool IsSmoother => true;

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Fast Limit", ParameterType.Double, 0.5);
            AddParameter("Slow Limit", ParameterType.Double, 0.05);
        }

        //companion
        public override List<string> Companions
        {
            get
            {
                List<string> c = new List<string>();
                c.Add("FAMA");
                return c;
            }
        }

        //populate
        public override void Populate()
        {
            //gather parameters values
            TimeSeries source = Parameters[0].AsTimeSeries;
            double fast = Parameters[1].AsDouble;
            double slow = Parameters[2].AsDouble;
            DateTimes = source.DateTimes;

            //look for cached calculator
            string key = "MAMA(" + fast + "," + slow + ")";
            if (!source.Cache.ContainsKey(key))
                source.Cache[key] = new MamaFamaCalculator(source, fast, slow);
            MamaFamaCalculator mfc = source.Cache[key] as MamaFamaCalculator;
            Values = mfc.MAMA.Values;
            PrefillNan(45);
        }
    }
}