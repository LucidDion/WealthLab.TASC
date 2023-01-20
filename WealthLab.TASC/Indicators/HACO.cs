﻿using WealthLab.Core;
using WealthLab.Indicators;

namespace WealthLab.TASC
{
    public class HACO : IndicatorBase
    {
        //parameterless constructor
        public HACO() : base()
        {
        }

        //for code based construction
        public HACO(BarHistory source, Int32 period, Int32 timeout)
        : base()
        {
            Parameters[0].Value = source;
            Parameters[1].Value = period;
            Parameters[2].Value = timeout;

            Populate();
        }

		//static method
		public static HACO Series(BarHistory source, int period, int timeout)
		{
			string key = CacheKey("HACO", period);
			if (source.Cache.ContainsKey(key))
				return (HACO)source.Cache[key];
			HACO h = new HACO(source, period, timeout);
			source.Cache[key] = h;
			return h;
		}


		//generate parameters
		protected override void GenerateParameters()
        {
            AddParameter("Source", ParameterType.BarHistory, null);
            AddParameter("Period", ParameterType.Int32, 34);
            AddParameter("Timeout", ParameterType.Int32, 2);
        }

		//populate
		public override void Populate()
        {
            BarHistory bars = Parameters[0].AsBarHistory;
            Int32 period = Parameters[1].AsInt;
            Int32 timeout = Parameters[2].AsInt;

            DateTimes = bars.DateTimes;

            if (period <= 0 || bars.Count == 0)
                return;

			int setup1bar = -1, setup2bar = -1, keepallBar = -1, keepallBard = -1, keepingdBar = -1, keepingBar = -1, utrBar = -1, dtrBar = -1;
            bool keep1 = false, keep2 = false, keep3 = false, keeping = false, keepall = false, keep1d = false, keep2d = false, keep3d = false, 
                keepingd = false, keepalld = false, utr = false, dtr = false, upw = true, dnw = true, result = true;

			/* Create Heikin-Ashi candles */
			
			var HO = bars.Open + 0;
			var HH = bars.High + 0;
			var HL = bars.Low + 0;
			var HC = (bars.Open + bars.High + bars.Low + bars.Close) / 4;
			var haC = HC + 0;
			//haC.Description = "Heikin-Ashi Close";
			
			for (int bar = 1; bar < bars.Count; bar++)
			{
				double o1 = HO[ bar - 1 ]; 
				double c1 = HC[ bar - 1 ]; 
				HO[bar] = ( o1 + c1 ) / 2; 
				HH[bar] = Math.Max( HO[bar], bars.High[bar] ); 
				HL[bar] = Math.Min( HO[bar], bars.Low[bar] );
				haC[bar] = ( HC[bar] + HO[bar] + HH[bar] + HL[bar] ) / 4;
			}

			/* Create "Crossover Formula" */
			
			var TMA1 = TEMA_TASC.Series( haC, period);
			var TMA2 = TEMA_TASC.Series( TMA1, period);
			var Diff = TMA1 - TMA2;
			var ZlHa = TMA1 + Diff;
            var AveragePrice = (bars.High + bars.Low) / 2;
            TMA1 = TEMA_TASC.Series( AveragePrice, period);
			TMA2 = TEMA_TASC.Series( TMA1, period);
			Diff = TMA1 - TMA2;
			var ZlCl = TMA1 + Diff;
			var ZlDif = ZlCl - ZlHa;
            //ZlDif.Description = "Crossover formula (" + period + ")";

            for (int bar = period; bar < bars.Count; bar++)
			{
				/* Create green candle */

				// Metastock Alert function resource:
				// http://www.meta-formula.com/metastock-alert-function.html

				if( !keep1 )
				{
					if( haC[bar] >= HO[bar] )	
					{
						keep1 = true;
						setup1bar = bar;
					}
				}
				if( keep1 )
				{
					keep1 = bar + 1 - setup1bar < timeout;
				}
				
				keep2 = ( ZlDif[bar] >= 0 );
				keeping = ( keep1 || keep2 );

                // Save bar when "keeping" is true
				if( keeping ) keepingBar = bar; else keepingBar = 0;

				keepall = keeping || (
                    // Implements Ref(keeping,-1)
					(( keepingBar > 0 ) & ( bar == keepingBar+1 )) &
					(bars.Close[bar] >= bars.Open[bar]) | (bars.Close[bar] >= bars.Close[bar-1]) );

                // Save bar when "keepall" is true
                if (keepall == true) keepallBar = bar; else keepallBar = 0;
			
				keep3 = ( ( bars.Close-bars.Open ).Abs()[bar] < ( (bars.High[bar]-bars.Low[bar]) * 0.35 ) ) & 
					( bars.High[bar] >= bars.Low[bar-1] );
				
				utr = keepall ||
                    // Implements Ref(keepall,-1)
					( ( keepallBar > 0 ) & ( bar == keepallBar+1 ) & keep3 );
				if( utr == true ) utrBar = bar; else utrBar = 0;
				
				
				/* Create red candle */

                // Metastock Alert function resource:
                // http://www.meta-formula.com/metastock-alert-function.html

				if( !keep1d )
				{
					if( haC[bar] < HO[bar] )
					{
						keep1d = true;
						setup2bar = bar;
					}
				}
				if( keep1d )
				{
					keep1d = bar + 1 - setup2bar < timeout;
				}
				
				keep2d = ( ZlDif[bar] < 0 );
				keep3d = ( ( bars.Close-bars.Open ).Abs()[bar] < ( (bars.High[bar]-bars.Low[bar]) * 0.35 ) ) & 
					( bars.Low[bar] <= bars.High[bar-1] );

				keepingd = ( keep1d || keep2d );
                
                // Save bar when "keeping" is true
				if( keepingd ) keepingdBar = bar; else keepingdBar = 0;	
				
				keepalld = keepingd || (
                    // Implements Ref(keeping,-1)
					(( keepingdBar > 0 ) & ( bar == keepingdBar+1 )) &	
					(bars.Close[bar] < bars.Open[bar]) | (bars.Close[bar] < bars.Close[bar-1]) );

                // Save bar when "keepall" is true
				if( keepalld == true ) keepallBard = bar; else keepallBard = 0;	
				
				dtr = keepalld ||
                    // Implements Ref(keepall,-1)
					( ( keepallBard > 0 ) & ( ( bar == keepallBard+1 ) & keep3d ) );
				if( dtr == true ) dtrBar = bar; else dtrBar = 0;

				upw = !dtr || ( ( ( dtrBar > 0 ) & ( bar == dtrBar+1 ) ) & utr );
				dnw = !utr || ( ( ( utrBar > 0 ) & ( bar == utrBar+1 ) ) & dtr );
				
				// Metastock's PREV is statement-based
				result = ( upw ) ? true : ( dnw ) ? false : result;

				/* HACO */
				
				if( result ) Values[bar] = 1; else Values[bar] = 0;
			}
		}

        public override string Name => "HACO";

        public override string Abbreviation => "HACO";

        public override string HelpDescription => "HACO (Heikin-Ashi Candlestick Oscillator) by Sylvain Vervoort from the December 2008 issue of Technical Analysis of Stocks & Commodities magazine.";

        public override string PaneTag => @"HACO";

        public override WLColor DefaultColor => WLColor.BlueViolet;

        public override PlotStyle DefaultPlotStyle => PlotStyle.Line;

		//slow in Indicator Profiler and intraday charts
		public override bool IsCalculationLengthy => true;
	}    
}