using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class RS2T : IndicatorBase
    {
        //constructors
        public RS2T() : base()
        {
        }
        public RS2T(BarHistory source, string indexSymbol = "SPY", int period = 10) : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = indexSymbol;
            Populate();
        }

        //static method
        public static RS2T Series(BarHistory source, string indexSymbol = "SPY", int period = 10)
        {
            string key = CacheKey("RS2T", indexSymbol, period);
            if (source.Cache.ContainsKey(key))
                return (RS2T)source.Cache[key];
            RS2T r = new RS2T(source, indexSymbol, period);
            source.Cache[key] = r;
            return r;
        }


        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            Parameter p = AddParameter("Index Symbol", ParameterType.String, "SPY");
            p.Uppercase = true;
            AddParameter("Period", ParameterType.Int32, 10);
        }

        //Name
        public override string Name => "RS2T";

        //Abbreviation
        public override string Abbreviation => "RS2T";

        //description
        public override string HelpDescription => "RS2t from the September 2020 issue produces a value that can be used to rank securities over several time frames.";

        //pane tag
        public override string PaneTag => "RS2T";

        //color
        public override WLColor DefaultColor => WLColor.Brown;

        //populate
        public override void Populate()
        {
            BarHistory source = Parameters[0].AsBarHistory;
            string indexSymbol = Parameters[1].AsString;
            int period = Parameters[2].AsInt;
            DateTimes = source.DateTimes;

            SymbolData indexC = SymbolData.Series(source, indexSymbol, PriceComponent.Close);
            TimeSeries rs = source.Close / indexC;

            TimeSeries fast = EMA.Series(rs, period);
            TimeSeries medium = SMA.Series(fast, 7);
            TimeSeries slow = SMA.Series(fast, 15);
            TimeSeries vslow = SMA.Series(slow, 30);

            for(int n = vslow.FirstValidIndex; n < source.Count; n++)
            {
                int tier1 = fast[n] >= medium[n] && medium[n] >= slow[n] && slow[n] >= vslow[n] ? 10 : 0;
                int tier2 = fast[n] >= medium[n] && medium[n] >= slow[n] && slow[n] < vslow[n] ? 9 : 0;
                int tier3 = fast[n] < medium[n] && medium[n] >= slow[n] && slow[n] >= vslow[n] ? 9 : 0;
                int tier4 = fast[n] < medium[n] && medium[n] >= slow[n] && slow[n] < vslow[n] ? 5 : 0;
                Values[n] = tier1 + tier2 + tier3 + tier4;
            }
            PrefillNan(period);
        }
    }
}