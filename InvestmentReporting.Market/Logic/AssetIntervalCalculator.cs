using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.Logic {
	public sealed class AssetIntervalCalculator {
		readonly ILogger           _logger;
		readonly AssetPriceManager _priceManager;

		public AssetIntervalCalculator(
			ILogger<AssetIntervalCalculator> logger, AssetPriceManager priceManager) {
			_logger       = logger;
			_priceManager = priceManager;
		}

		public IReadOnlyCollection<(DateTime, DateTime)> TryCalculateRequiredIntervals(AssetISIN isin) {
			var startDate = GetStartDate(isin);
			var endDate   = GetEndDate();
			_logger.LogTrace($"Interval for ISIN '{isin}' is {startDate}-{endDate}");
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

		DateTime GetStartDate(AssetISIN isin) {
			var price = _priceManager.TryGet(isin);
			if ( price == null ) {
				_logger.LogTrace($"Asset price for ISIN '{isin}' is unknown, lookup first add date");
				return GetFirstAddDate(isin);
			}
			var lastRecordedDate = price.Candles[^1].Date;
			var nextDay          = lastRecordedDate.Date.AddDays(1);
			_logger.LogTrace($"Last recorded date for ISIN '{isin}' is {lastRecordedDate}, use next day ({nextDay})");
			return nextDay;
		}

		DateTime GetFirstAddDate(AssetISIN isin) {
			var addCommand = _priceManager.GetAddAssetCommands(isin)
				.FirstOrDefault();
			if ( addCommand != null ) {
				var date = addCommand.Date.Date;
				_logger.LogTrace($"Add commands found for ISIN '{isin}': {date}");
				return date;
			}
			_logger.LogError($"No add commands found for ISIN '{isin}', use yesterday date");
			return GetEndDate().AddDays(-1);
		}

		DateTime GetEndDate() =>
			DateTimeOffset.UtcNow.AddDays(-1).Date;

	}
}