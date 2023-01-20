using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class VPN : IndicatorBase
    {
        //parameterless constructor
        public VPN() : base()
        {
            OverboughtLevel = 80;
            OversoldLevel = 20;
        }

        //for code based construction
        public VPN(BarHistory bars, Int32 vpPeriod, Int32 smoothPeriod)
            : base()
        {
            Parameters[0].Value = bars;
            Parameters[1].Value = vpPeriod;
            Parameters[2].Value = smoothPeriod;
            OverboughtLevel = +80;
            OversoldLevel = -80;
            Populate();
        }

        //static method
        public static VPN Series(BarHistory source, int vpPeriod, int smoothPeriod)
        {
            string key = CacheKey("VPN", vpPeriod, smoothPeriod);
            if (source.Cache.ContainsKey(key))
                return (VPN)source.Cache[key];
            VPN s = new VPN(source, vpPeriod, smoothPeriod);
            source.Cache[key] = s;
            return s;
        }

        //generate parameters
        protected override void GenerateParameters()
        {
            AddParameter("Bars", ParameterType.BarHistory, null);
            AddParameter("VP period", ParameterType.Int32, 30);
            AddParameter("Smoothing period", ParameterType.Int32, 3);
        }

        public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 vpPeriod = Parameters[1].AsInt;
            Int32 smoothPeriod = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;
            var period = Math.Max(vpPeriod, smoothPeriod);

            if (period <= 0 || DateTimes.Count == 0)
                return;

            var Avg = bars.AveragePriceHLC;
            var V = bars.Volume;

            TimeSeries MAV = SMA.Series(V, vpPeriod);
            TimeSeries MF = Avg - (Avg >> 1);
            TimeSeries MC = 0.1 * ATR.Series(bars, vpPeriod);

            TimeSeries VP = new TimeSeries(DateTimes, 0);
            TimeSeries VMP = new TimeSeries(DateTimes, 0);
            TimeSeries VMN = new TimeSeries(DateTimes, 0);
            TimeSeries VN = new TimeSeries(DateTimes, 0);

            for (int bar = 0; bar < bars.Count; bar++)
            {
                if (bar == 0)
                {
                    VMP[bar] = VMN[bar] = 0;
                }

                VMP[bar] = MF[bar] > MC[bar] ? V[bar] : 0;
                VMN[bar] = MF[bar] < -MC[bar] ? V[bar] : 0;
            }

            VP = VMP.Sum(vpPeriod);
            VN = VMN.Sum(vpPeriod);

            TimeSeries VPN = (VP - VN) / MAV / vpPeriod * 100;
            VPN = EMA.Series(VPN, smoothPeriod);

            Values = VPN.Values;
        }

        public override string Name => "VPN";

        public override string Abbreviation => "VPN";

        public override string HelpDescription => "VPN (Volume Positive Negative) indicator by Markos Katsanos from April 2021 issue of Technical Analysis of Stocks & Commodities magazine is a momentum volume oscillator that measures change in buying and selling pressure with a volatility filter, oscillating between -100 and 100.";

        public override string PaneTag => @"VPN";

        public override WLColor DefaultColor => WLColor.Blue;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;
    }    
}