using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class GannHiLoActivator : IndicatorBase
    {
        public override string Name => "Gann HiLo Activator";
        public override string Abbreviation => "GannHiLoActivator";
        public override string HelpDescription => "Created by Robert Krausz, the Gann HiLo Activator is a trend-following indicator. Its interpretation is similar to a moving average.";
        public override string PaneTag => "Price";
        public override WLColor DefaultColor => WLColor.DarkGreen;

        public GannHiLoActivator()
        {
        }
        public GannHiLoActivator(BarHistory source, int period = 3, bool starVersion = false)
        {
            base.Parameters[0].Value = source;
            base.Parameters[1].Value = period;
            base.Parameters[2].Value = starVersion;
            this.Populate();
        }

        //static method
        public static GannHiLoActivator Series(BarHistory source, int period, bool starVersion)
        {
            string key = CacheKey("GannHiLoActivator", period, starVersion);
            if (source.Cache.ContainsKey(key))
                return (GannHiLoActivator)source.Cache[key];
            GannHiLoActivator ghla = new GannHiLoActivator(source, period, starVersion);
            source.Cache[key] = ghla;
            return ghla;
        }

        protected override void GenerateParameters()
        {
            base.AddParameter("Source", ParameterType.BarHistory, null);
            base.AddParameter("Lookback Period", ParameterType.Int32, 3);
            base.AddParameter("Barbara Star's version", ParameterType.Boolean, false);
        }
        public override void Populate()
        {
            BarHistory bars = base.Parameters[0].AsBarHistory;
            int period = base.Parameters[1].AsInt;
            bool starVersion = base.Parameters[2].AsBoolean == true;
            DateTimes = bars.DateTimes;
            int FirstValidValue = period + 1;
            if (bars.Count < FirstValidValue)
            {
                return;
            }

            TimeSeries smaH = FastSMA.Series(bars.High, period);
            TimeSeries smaL = FastSMA.Series(bars.Low, period);

            for (int bar = 0; bar < FirstValidValue; bar++)
            {
                Values[bar] = 0;
            }

            for (int i = FirstValidValue; i < bars.Count; i++)
            {                
                if (!starVersion)
                    Values[i] = (bars.Close[i] < smaL[i - 1]) ? smaH[i] : smaL[i];
                else
                {
                    if (bars.Close[i] < smaL[i - 1])
                        Values[i] = smaH[i];
                    else
                    if (bars.Close[i] > smaH[i - 1])
                        Values[i] = smaL[i];
                    else
                        Values[i] = Values[i - 1];
                }
            }
            PrefillNan(period + 1);
        }
    }
}