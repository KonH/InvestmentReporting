using System;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.Logic {
	public sealed class ExchangeManager {
		readonly ILogger              _logger;
		readonly CurrencyPriceManager _priceManager;

		public ExchangeManager(ILogger<ExchangeManager> logger, CurrencyPriceManager priceManager) {
			_logger       = logger;
			_priceManager = priceManager;
		}

		public decimal Exchange(CurrencyCode from, CurrencyCode to, decimal amount, DateTimeOffset date) {
			var result = amount * _priceManager.GetPriceAt(from, to, date);
			_logger.LogTrace($"Exchange {amount} ({from}) to {result} ({to})");
			return result;
		}
	}
}