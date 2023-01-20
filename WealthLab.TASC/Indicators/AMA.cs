﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class AMA : IndicatorBase
    {
        //constructors
        public AMA() : base()
        {
        }
        public AMA(BarHistory source, int period = 10, int fastPeriod = 2, int slowPeriod = 30) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = fastPeriod;
            Parameters[3].Value = slowPeriod;
            Populate();
        }

        //static method
        public static AMA Series(BarHistory source, int period = 10, int fastPeriod = 2, int slowPeriod = 30)
        {
            string key = CacheKey("AMA", period, fastPeriod, slowPeriod);
            if (source.Cache.ContainsKey(key))
                return (AMA)source.Cache[key];
            AMA ama = new AMA(source, period, fastPeriod, slowPeriod);
            source.Cache[key] = ama;
            return ama;
        }

        //Name
        public override string Name
        {
            get
            {
                return "Adaptive Moving Average";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "AMA";
            }
        }

        //help
        public override string HelpDescription
        {
            get
            {
                return "Adaptive Moving Average, based on the article by Vitali Apirine in the April 2018 issue of Stocks & Commodities magazine.";
            }
        }

        //plot in source pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //color
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.MediumVioletRed;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 10);
            AddParameter("Fastest EMA", ParameterType.Int32, 2);
            AddParameter("Slowest EMA", ParameterType.Int32, 30);
        }

        //populate
        public override void Populate()
        {
            //get parameter values
            BarHistory source = Parameters[0].AsBarHistory;
            int period = Parameters[1].AsInt;
            int fastEMA = Parameters[2].AsInt;
            int slowEMA = Parameters[3].AsInt;
            DateTimes = source.DateTimes;
            if (source.Count < period + 1)
                return;

            //calculate AMA, populate
            double fastSC = 2.0 / (fastEMA + 1.0);
            double slowSC = 2.0 / (slowEMA + 1.0);
            double priorAMA = source.Close[period];
            for(int n = period + 1; n < source.Count; n++)
            {
                //MLTP
                double mltp = 0.5;
                double c = source.Close[n];
                double hh = Highest.Value(n, source.High, period + 1);
                double ll = Lowest.Value(n, source.Low, period + 1);
                double diff = hh - ll;
                if (diff != 0 && !Double.IsNaN(diff))
                    mltp = Math.Abs((c - ll) - (hh - c)) / diff;

                //Smoothing constant
                double ssc = mltp * (fastSC - slowSC) + slowSC;
                double constant = Math.Pow(ssc, 2.0);

                //current AMA
                Values[n] = priorAMA + constant * (c - priorAMA);
                priorAMA = Values[n];
            }
        }
    }
}