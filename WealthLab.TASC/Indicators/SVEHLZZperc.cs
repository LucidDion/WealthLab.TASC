using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class SVEHLZZperc : IndicatorBase
    {
        public override string Name => "SVEHLZZperc";

        public override string Abbreviation => "SVEHLZZperc";

        public override string HelpDescription => "Sylvain Vervoort's SVEHLZZperc zigzag indicator from June 2013 issue of Stocks & Commodities magazine is a trailing reverse indicator that is based on a percent price change between high and low prices, or uses an ATR volatility factor, or both of them combined.";

        public override string PaneTag => @"SVEHLZZperc";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.ThickLine;

        //parameterless constructor
        public SVEHLZZperc() : base()
        {
        }

        //for code based construction
        public SVEHLZZperc(BarHistory bars, double change, int period, double factor, SVEHLZZpercType type)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = change;
            Parameters[2].Value = period;
            Parameters[3].Value = factor;
            Parameters[4].Value = type;

            Populate();            
        }

        //static method
        public static SVEHLZZperc Series(BarHistory source, double change, int period, double factor, SVEHLZZpercType type)
        {
            string key = CacheKey("SVEHLZZperc", change, period, factor, type);
            if (source.Cache.ContainsKey(key))
                return (SVEHLZZperc)source.Cache[key];
            SVEHLZZperc sve = new SVEHLZZperc(source, change, period, factor, type);
            source.Cache[key] = sve;
            return sve;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Price Change %", ParameterType.Double, 5);
            AddParameter("ATR Period", ParameterType.Int32, 5);
            AddParameter("ATR Factor", ParameterType.Double, 1.5);
            Parameter p = AddParameter("Type", ParameterType.StringChoice, "ATR");
            p.Choices.Add("Percent");
            p.Choices.Add("ATR");
            p.Choices.Add("Combined");
            p.Choices.Add("Point");
            p.TypeName = "SVEHLZZpercType";
        }

        //populate
        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Double change = Parameters[1].AsDouble;
            Int32 period = Parameters[2].AsInt;
            Double factor = Parameters[3].AsDouble;
            SVEHLZZpercType type = (SVEHLZZpercType)Enum.Parse(typeof(SVEHLZZpercType), Parameters[4].AsString);

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;
            
            int Trend = 0;
            double Reverse = 0, HPrice = 0, LPrice = 0;

            ATR atr = new ATR(bars, period);

            for (int bar = period; bar < bars.Count; bar++)
            {
                double atrValue = atr[bar] * factor;

                if (Trend >= 0)
                {
                    HPrice = Math.Max(bars.High[bar], HPrice);
                    switch (type)
                    {
                        case SVEHLZZpercType.Percent:
                            Reverse = HPrice * (1 - change * 0.01);
                            break;
                        case SVEHLZZpercType.ATR:
                            Reverse = HPrice - atrValue;
                            break;
                        case SVEHLZZpercType.Combined:
                            Reverse = HPrice - (HPrice * (change * 0.01) + atrValue);
                            break;
                        case SVEHLZZpercType.Point:
                            double tickSize = bars.SymbolInfo == null ? 0.01 : bars.SymbolInfo.TickSize;
                            Reverse = HPrice - change * tickSize;
                            break;
                        default:
                            break;
                    }

                    if (bars.Low[bar] <= Reverse)
                    {
                        Trend = -1;
                        LPrice = bars.Low[bar];
                        
                        switch (type)
                        {
                            case SVEHLZZpercType.Percent:
                                Reverse = LPrice * (1 + change * 0.01);
                                break;
                            case SVEHLZZpercType.ATR:
                                Reverse = LPrice + atrValue;
                                break;
                            case SVEHLZZpercType.Combined:
                                Reverse = LPrice + (atrValue + LPrice * (change * 0.01));
                                break;
                            case SVEHLZZpercType.Point:
                                double tickSize = bars.SymbolInfo == null ? 0.01 : bars.SymbolInfo.TickSize;
                                Reverse = LPrice + change * tickSize;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (Trend <= 0)
                {
                    LPrice = Math.Min(bars.Low[bar], LPrice);
                    switch (type)
                    {
                        case SVEHLZZpercType.Percent:
                            Reverse = LPrice * (1 + change * 0.01);
                            break;
                        case SVEHLZZpercType.ATR:
                            Reverse = LPrice + atrValue;
                            break;
                        case SVEHLZZpercType.Combined:
                            Reverse = LPrice + (atrValue + LPrice * (change * 0.01));
                            break;
                        case SVEHLZZpercType.Point:
                            double tickSize = bars.SymbolInfo == null ? 0.01 : bars.SymbolInfo.TickSize;
                            Reverse = LPrice + change * tickSize;
                            break;
                        default:
                            break;
                    }
                    
                    if (bars.High[bar] >= Reverse)
                    {
                        Trend = 1;
                        HPrice = bars.High[bar];

                        switch (type)
                        {
                            case SVEHLZZpercType.Percent:
                                Reverse = HPrice * (1 - change * 0.01);
                                break;
                            case SVEHLZZpercType.ATR:
                                Reverse = HPrice - atrValue;
                                break;
                            case SVEHLZZpercType.Combined:
                                Reverse = HPrice - (HPrice * (change * 0.01) + atrValue);
                                break;
                            case SVEHLZZpercType.Point:
                                double tickSize = bars.SymbolInfo == null ? 0.01 : bars.SymbolInfo.TickSize;
                                Reverse = HPrice - change * tickSize;
                                break;
                            default:
                                break;
                        }
                    }
                }
                Values[bar] = Reverse;
            }
        }
    }
}