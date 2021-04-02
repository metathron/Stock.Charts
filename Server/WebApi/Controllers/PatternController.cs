using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Skender.Stock.Indicators;
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
        [HttpGet("StandardSignals/{pair}", Name = "GetAllStandardSignalsPair")]
        [HttpGet("StandardSignals/{pair}/{periodSize}", Name = "GetAllStandardSignalsPeriode")]
        [HttpGet("StandardSignals/{pair}/{periodSize}/{fromTicks}/{toTicks}", Name = "GetAllStandardSignalsPairData")]
        public IEnumerable<object> GetAllStandardSignals(string pair = "BTCUSDT", PeriodSize periodSize = PeriodSize.ThirtyMinutes, long? fromTicks = null, long? toTicks = null)
        {

            DateTime from = DateTime.Now.AddDays(-2);
            DateTime to = DateTime.Now;

            if (fromTicks.HasValue)
                from = new DateTime(fromTicks.Value);
            if (toTicks.HasValue)
                to = new DateTime(toTicks.Value);

            if (to > DateTime.Now)
                to = DateTime.Now;

            IEnumerable<PatternQuote> history = HistoryService.GetHistory(pair, periodSize, from, to).Select(s => ToDerived<Quote, PatternQuote>(s));

            var result = Indicator.GetBearishEngulfing(history);
            result = result.Union(Indicator.GetDarkCloudCover(history));  // "date": "2021-01-14T15:30:00Z"  not confirmed ony 2 Candles
            result = result.Union(Indicator.GetEveningStar(history));
            result = result.Union(Indicator.GetHangingMan(history));  //   "date": "2021-01-15T22:00:00Z"  ??? highest?
            result = result.Union(Indicator.GetShootingStar(history));  // "date": "2021-01-16T00:30:00Z" 
            result = result.Union(Indicator.GetThreeBlackCrows(history));
            result = result.Union(Indicator.GetBullishEngulfing(history));
            result = result.Union(Indicator.GetHammer(history));   // "date": "2021-01-15T16:00:00Z"
            result = result.Union(Indicator.GetInverseHammer(history));  //  "date": "2021-01-15T04:30:00Z"
            result = result.Union(Indicator.GetMorningStar(history));
            result = result.Union(Indicator.GetPiercingLine(history)); // 14.01.2021 10UTC
            result = result.Union(Indicator.GetThreeWhiteSoldiers(history)); //"date": "2021-01-14T14:00:00Z"
            result = result.Union(Indicator.GetDragonflyDoji(history));  //  "date": "2021-01-17T17:30:00Z"
            result = result.Union(Indicator.GetGraveStoneDoji(history));
            result = result.Union(Indicator.GetLongLeggedDoji(history));  //  "date": "2021-01-15T01:00:00Z"
            result = result.Union(Indicator.GetMarubozu(history));  //"source": "Marubozu open bullish", "date": "2021-01-14T17:00:00Z"
                                                                    //  result = result.Union(Indicator.GetSpinningTop(history));   // "date": "2021-01-17T08:00:00Z"


            var groupedPatterns = result.GroupBy(r => r.Source).ToDictionary(x=>x.Key,y=>y.ToList());
            //bool isFirstElemet = true;


            //List<object[]> returnValue = new List<object[]>();

            //foreach (var v in history)
            //{
            //    if (isFirstElemet)
            //    {
            //        isFirstElemet = false;

            //        List<object> curHeader = new List<object>();
            //        curHeader.Add("Date");
            //        curHeader.Add("Low");
            //        curHeader.Add("Open");
            //        curHeader.Add("Close");
            //        curHeader.Add("High");
            //        foreach (var key in groupedPatterns)
            //        {
            //            curHeader.Add(key.Key);
            //        }
            //        returnValue.Add(curHeader.ToArray());

            //    }
            //    List<object> curEntry = new List<object>();
            //    curEntry.Add(v.Date);
            //    curEntry.Add(v.Low);
            //    curEntry.Add(v.Open);
            //    curEntry.Add(v.Close);
            //    curEntry.Add(v.High);
            //    foreach (var key in groupedPatterns)
            //    {
            //        var pattern = key.FirstOrDefault(p => p.Date == v.Date);
            //        if (pattern == null)
            //            curEntry.Add(null);
            //        else
            //            curEntry.Add(pattern.Candle.High);
            //    }

            //    returnValue.Add(curEntry.ToArray());

            //}

            return new List<object>() {history.Select(s=>s as IQuote), groupedPatterns };
        }

    }
}
