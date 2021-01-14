using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skender.Stock.Indicators;

namespace BinanceConnector
{
    public class Spot
    {
        public Spot()
        {
            Initialize();
        }
        public void Initialize()
        {
            string apiKey = "";
            string apiSecret = "";
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(apiKey, apiSecret),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });
            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(apiKey, apiSecret),
                LogVerbosity = LogVerbosity.Debug,
                LogWriters = new List<TextWriter> { Console.Out }
            });
        }
        public static List<Quote> GetChartData(string pair, PeriodSize interval, DateTime from, DateTime to)
        {
            List<Quote> result = new List<Quote>();
            using (var client = new BinanceClient())
            {
                var res1 = client.Spot.Market.GetKlines(pair, (KlineInterval)((int)interval), from, to).Data.ToList();
                result = res1.Select(s => new Quote() { Close = s.Close, Date = s.OpenTime, High = s.High, Low = s.Low, Open = s.Open, Volume = s.BaseVolume }).ToList();
            }
            return result;
        }
    }
}
