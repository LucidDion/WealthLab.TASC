using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class ATRTrail : IndicatorBase
    {
        //parameterless constructor
        public ATRTrail() : base()
        {
        }

        //for code based construction
        public ATRTrail(BarHistory source, Int32 period, Double factor)
            : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = factor;
            Populate();
        }

        //static method
        public static ATRTrail Series(BarHistory source, int period, double factor)
        {
            string key = CacheKey("ATRTrail", period, factor);
            if (source.Cache.ContainsKey(key))
                return (ATRTrail)source.Cache[key];
            ATRTrail atrTrail = new ATRTrail(source, period, factor);
            source.Cache[key] = atrTrail;
            return atrTrail;
        }

        //name
        public override string Name
        {
            get
            {
                return "ATRTrail";
            }
        }

        //abbreviation
        public override string Abbreviation
        {
            get
            {
                return "ATRTrail";
            }
        }

        //description
        public override string HelpDescription
        {
            get
            {
                return "ATR Trailing Stop by Sylvain Vervoort.";
            }
        }

        //price pane
        public override string PaneTag
        {
            get
            {
                return "Price";
            }
        }

        //default color
        public override WLColor DefaultColor
        {
            get
            {
                return WLColor.DarkRed;
            }
        }

        //default plot style
        public override PlotStyle DefaultPlotStyle
        {
            get
            {
                return PlotStyle.Line;
            }
        }

        //populate
        public override void Populate()
        {
            BarHistory source = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Double factor = Parameters[2].AsDouble;
            DateTimes = source.DateTimes;

            //ATR
            ATR atr = new ATR(source, period);

            //calculate ATR Trailing Stop
            for (int n = period; n < source.Count; n++)
            {
                double loss = factor * atr[n];
                if (source.Close[n] > Values[n - 1] && source.Close[n - 1] > Values[n - 1])
                    Values[n] = Math.Max(Values[n - 1], source.Close[n] - loss);
                else if (source.Close[n] < Values[n - 1] && source.Close[n - 1] < Values[n - 1])
                    Values[n] = Math.Min(Values[n - 1], source.Close[n] + loss);
                else
                    Values[n] = source.Close[n] > Values[n - 1] ? source.Close[n] - loss : source.Close[n] + loss;
            }
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 5);
            AddParameter("Factor", ParameterType.Double, 3.6);
        }
    }
}