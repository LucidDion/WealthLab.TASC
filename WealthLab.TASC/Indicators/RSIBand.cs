using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RSIBand : IndicatorBase
    {
        //parameterless constructor
        public RSIBand() : base()
        {
        }

        //for code based construction
        public RSIBand(TimeSeries source, Int32 period, Double target)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;

            Populate();
        }

        //static method
        public static RSIBand Series(TimeSeries source, int period, double target)
        {
            string key = CacheKey("RSIBand", period, target);
            if (source.Cache.ContainsKey(key))
                return (RSIBand)source.Cache[key];
            RSIBand rb = new RSIBand(source, period, target);
            source.Cache[key] = rb;
            return rb;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.TimeSeries, PriceComponent.Close);
            AddParameter("Period", ParameterType.Int32, 14);
            AddParameter("RSI Target Level", ParameterType.Double, 30d);
        }

        //populate
        public override void Populate()
        {
            TimeSeries ds = Parameters[0].AsTimeSeries;
            Int32 period = Parameters[1].AsInt;
            Double target = Parameters[2].AsDouble;

            DateTimes = ds.DateTimes;

            if (period <= 0 || ds.Count == 0)
                return;

            var FirstValidValue = period - 1 + ds.FirstValidIndex;

            double HypotheticalCloseToMatchRSITarget = 0d;
            var _P = 0d;
            var _N = 0d;
            Values[0] = ds[0];
            for (int bar = 1; bar < ds.Count; bar++)
            {
                // Standard RSI code
                double diff = ds[bar] - ds[bar - 1];
                double W = 0d;
                double S = 0d;
                if (diff > 0) W = diff;
                if (diff < 0) S = -diff;

                // Compute the hypothetical price close to reach the target RSI level based on yesterday’s RSI and close
                // Depending on if we would need the price to increase or decrease, we use a different formula
                if (this[bar-1] > ds[bar-1])
                    HypotheticalCloseToMatchRSITarget = ds[bar-1] + _P - _P * period - ((_N * period) - _N) * target/ (target - 100);
                else
                    HypotheticalCloseToMatchRSITarget = ds[bar-1] - _N - _P + _N * period + _P * period + (100 * _P) / target - (100 * _P * period) / target;

                // Clamping code to keep the RSI Bands within roughly 10% of the price				
                if ((HypotheticalCloseToMatchRSITarget - ds[bar]) > 0.1 * ds[bar])
                    HypotheticalCloseToMatchRSITarget = ds[bar] * 1.1;
                else if ((HypotheticalCloseToMatchRSITarget - ds[bar]) < -0.1 * ds[bar])
                    HypotheticalCloseToMatchRSITarget = ds[bar] * 0.9;

                // Resume standard RSI code to update the running _P and _N averages
                _P = ((period - 1) * _P + W) / period;
                _N = ((period - 1) * _N + S) / period;

                Values[bar] = HypotheticalCloseToMatchRSITarget;
            }
            PrefillNan(period);
        }

        public override string Name => "RSIBand";

        public override string Abbreviation => "RSIBand";

        public override string HelpDescription => "Based on the indicator by François Bertrand published in the April 2008 issue of Stocks and Commodities Magazine. RSI bands are calculated by answering the question, \"What price should the stock have to reach today to be considered overbought/oversold, given yesterday’s RSI?\"";

        public override string PaneTag => @"RSIBand";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}