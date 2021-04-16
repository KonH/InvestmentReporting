using System;
using InvestmentReporting.Market.Entity;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.Logic {
	public sealed class CurrencyIntervalCalculator {
		readonly ILogger              _logger;
		readonly CurrencyPriceManager _priceManager;

		public CurrencyIntervalCalculator(
			ILogger<CurrencyIntervalCalculator> logger, CurrencyPriceManager priceManager) {
			_logger       = logger;
			_priceManager = priceManager;
		}

		public Tuple<DateTime, DateTime>? TryCalculateRequiredInterval(AssetFIGI figi) {
			var startDate = GetStartDate(figi);
			var endDate   = GetEndDate();
			_logger.LogTrace($"Interval for FIGI '{figi}' is {startDate}-{endDate}");
			var diff = (endDate - startDate);
			if ( diff < TimeSpan.FromDays(1) ) {
				_logger.LogTrace("No processing required");
				return null;
			}
			return new Tuple<DateTime, DateTime>(startDate, endDate);
		}

		DateTime GetStartDate(AssetFIGI figi) {
			var price = _priceManager.TryGet(figi);
			if ( price == null ) {
				_logger.LogTrace($"Currency price for FIGI '{figi}' is unknown, lookup first add date");
				return _priceManager.GetStartDate();
			}
			var lastRecordedDate = price.Candles[^1].Date;
			var nextDay          = lastRecordedDate.Date.AddDays(1);
			_logger.LogTrace($"Last recorded date for FIGI '{figi}' is {lastRecordedDate}, use next day ({nextDay})");
			return nextDay;
		}

		DateTime GetEndDate() =>
			DateTimeOffset.UtcNow.AddDays(-1).Date;

	}
}