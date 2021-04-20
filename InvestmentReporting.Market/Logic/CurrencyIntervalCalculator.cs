using System;
using System.Collections.Generic;
using System.Linq;
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

		public IReadOnlyCollection<(DateTime, DateTime)> TryCalculateRequiredIntervals(AssetFIGI figi) {
			var startDate = GetStartDate(figi);
			var endDate   = GetEndDate();
			_logger.LogTrace($"Interval for FIGI '{figi}' is {startDate}-{endDate}");
			var result = new List<(DateTime, DateTime)>();
			var diff   = (endDate - startDate);
			if ( diff < TimeSpan.FromDays(1) ) {
				_logger.LogTrace("No processing required");
				return result;
			}
			var date = startDate;
			while ( date < endDate ) {
				var interval = (endDate - date);
				var nextDate = (interval.TotalDays < 365) ? endDate : date.AddDays(365);
				result.Add((date, nextDate));
				date = nextDate.AddDays(1);
			}
			_logger.LogTrace(
				$"Interval splits into {result.Count} parts: {string.Join("; ", result.Select(r => r.ToString()))}");
			return result;
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