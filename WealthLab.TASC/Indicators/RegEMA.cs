using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    //RegEMA Indicator class
    public class RegEMA : IndicatorBase
    {
        //parameterless constructor
        public RegEMA() : base()
        {
        }

        //for code based construction
        public RegEMA(TimeSeries source, Double smoothing, Double regularization)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = smoothing;
            Parameters[2].Value = regularization;

            Populate();
        }

        //static method
        public static RegEMA Series(TimeSeries source, double smoothing, double regularization)
        {
            string key = CacheKey("RegEMA", smoothing, regularization);
            if (source.Cache.ContainsKey(key))
                return (RegEMA)source.Cache[key];
            RegEMA r = new RegEMA(source, smoothing, regularization);
            source.Cache[key] = r;
            return r;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Smoothing", ParameterType.Double, 0.91);
            AddParameter("Regularization", ParameterType.Double, 0.5);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Double smoothing = Parameters[1].AsDouble;
            Double regularization = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (ds.Count == 0)
                return;

            //Assign first bar that contains indicator data
            var FirstValidValue = ds.FirstValidIndex + 2;
            if (FirstValidValue > ds.Count) FirstValidValue = ds.Count;

            //Initialize start of series
            for (int bar = 0; bar < FirstValidValue; bar++)
                Values[bar] = ds[bar];

            //Rest of series
            for (int bar = FirstValidValue; bar < ds.Count; bar++)
            {
                double term1 = (1 + 2 * regularization) * this[bar - 1];
                double term2 = smoothing * (ds[bar] - this[bar - 1]);
                double term3 = regularization * this[bar - 2];
                Values[bar] = (term1 + term2 - term3) / (1 + regularization);
            }
        }

        public override string Name => "RegEMA";

        public override string Abbreviation => "RegEMA";

        public override string HelpDescription => "Regularized EMA indicator from Chris Satchwell, Ph.D. Published in the July 2003 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"Price";

        public override WLColor DefaultColor => WLColor.DarkGreen;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

        public override bool IsSmoother => true;
    }
}