using Microsoft.AspNetCore.Mvc;
using Skender.Stock.Indicators;
using Stock.CandleStickPatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("{Controller}")]
    public class PatternController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "API is functioning nominally.";
        }

        [HttpGet("patternHistory")]
        public IEnumerable<PatternQuote> GetQuotes()
        {
            return HistoryService.GetHistory().Select(s => s as PatternQuote);
        }

        static Dictionary<string, PropertyInfo> _PropInfos = new Dictionary<string, PropertyInfo>();
        public static TDerived ToDerived<TBase, TDerived>(TBase tBase)
    where TDerived : TBase, new()
        {
            TDerived tDerived = new TDerived();
            foreach (PropertyInfo propBase in typeof(TBase).GetProperties())
            {
                if (!_PropInfos.ContainsKey(propBase.Name))
                    _PropInfos.Add(propBase.Name, typeof(TDerived).GetProperty(propBase.Name));
                _PropInfos[propBase.Name].SetValue(tDerived, propBase.GetValue(tBase, null), null);
            }
            return tDerived;
        }

        [HttpGet("StandardSignals")]
        public IEnumerable<SignalResult> GetAllStandardSignals(
        [FromRoute] int lookbackPeriod, [FromRoute] decimal minimumRatioLowerToBody = 2.2M, [FromRoute] decimal maxBodySizeInPercent = 25.0M)
        {
            IEnumerable<PatternQuote> history = HistoryService.GetHistory("BTCUSDT", PeriodSize.ThirtyMinutes,DateTime.Now.AddDays(-5)).Select(s => ToDerived<Quote, PatternQuote>(s));

            var result = Stock.CandleStickPatterns.BearishEngulfing.GetSignals(history);
            result = result.Union(Stock.CandleStickPatterns.DarkCloudCover.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.EveningStar.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.HangingMan.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.ShootingStar.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.ThreeBlackCrows.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.BullishEngulfing.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.Hammer.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.InverseHammer.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.MorningStar.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.PiercingLine.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.ThreeWhiteSoldiers.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.DragonflyDoji.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.GraveStoneDoji.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.LongLeggedDoji.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.Marubozu.GetSignals(history));
            result = result.Union(Stock.CandleStickPatterns.SpinningTop.GetSignals(history));
            return result.OrderBy(o => o.Date);
        }

    }
}
