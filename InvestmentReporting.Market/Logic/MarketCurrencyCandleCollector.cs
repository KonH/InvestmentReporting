using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace InvestmentReporting.Market.Logic {
	public sealed class MarketCurrencyCandleCollector {
		readonly ILogger _logger;

		readonly CurrencyManager            _currencyManager;
		readonly CurrencyPriceManager       _priceManager;
		readonly CurrencyIntervalCalculator _intervalCalculator;

		public MarketCurrencyCandleCollector(
			ILogger<MarketCurrencyCandleCollector> logger,
			CurrencyManager currencyManager, CurrencyPriceManager priceManager,
			CurrencyIntervalCalculator intervalCalculator) {
			_logger             = logger;
			_currencyManager    = currencyManager;
			_priceManager       = priceManager;
			_intervalCalculator = intervalCalculator;
		}

		public async Task Collect(SandboxContext context) {
			var currencyFigis = new Dictionary<CurrencyCode, string> {
				[new("USD")] = "BBG0013HGFT4",
				[new("EUR")] = "BBG0013HJJ31"
			};
			foreach ( var currency in _currencyManager.GetAll() ) {
				if ( currency == "RUB" ) {
					continue;
				}
				if ( !currencyFigis.TryGetValue(currency, out var figi) ) {
					_logger.LogError($"Unknown currency: '{currency}'");
					continue;
				}
				var interval = _intervalCalculator.TryCalculateRequiredInterval(new(figi));
				if ( interval == null ) {
					continue;
				}
				var (startDate, endDate) = interval;
				var candles = await context.MarketCandlesAsync(
					figi, startDate, endDate, CandleInterval.Day);
				_logger.LogTrace($"Found currency candles: {string.Join("\n", candles.Candles.Select(c => c.ToString()))}");
				if ( candles.Candles.Count > 0 ) {
					await _priceManager.AddOrAppendCandles(currency, new(figi), candles);
				}
			}
		}
	}
}