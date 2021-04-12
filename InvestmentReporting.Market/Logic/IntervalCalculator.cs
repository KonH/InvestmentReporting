using System;
using System.Linq;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.Logic {
	public sealed class IntervalCalculator {
		readonly ILogger       _logger;
		readonly IStateManager _stateManager;
		readonly PriceManager  _priceManager;

		public IntervalCalculator(
			ILogger<IntervalCalculator> logger, IStateManager stateManager, PriceManager priceManager) {
			_logger       = logger;
			_stateManager = stateManager;
			_priceManager = priceManager;
		}

		public Tuple<DateTime, DateTime>? TryCalculateRequiredInterval(AssetISIN isin) {
			var startDate = GetStartDate(isin);
			var endDate   = GetEndDate();
			_logger.LogTrace($"Interval for ISIN '{isin}' is {startDate}-{endDate}");
			var diff = (endDate - startDate);
			if ( diff < TimeSpan.FromDays(1) ) {
				_logger.LogTrace("No processing required");
				return null;
			}
			return new Tuple<DateTime, DateTime>(startDate, endDate);
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
			var commands = _stateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
			var addCommand = commands
				.Select(c => c as AddAssetModel)
				.Where(c => c != null)
				.Select(c => c!)
				.Where(c => c.Isin == isin)
				.OrderBy(c => c)
				.FirstOrDefault();
			if ( addCommand != null ) {
				var date = addCommand.Date.Date;
				_logger.LogError($"Add commands found for ISIN '{isin}': {date}");
				return date;
			}
			_logger.LogError($"No add commands found for ISIN '{isin}', use yesterday date");
			return GetEndDate().AddDays(-1);
		}

		DateTime GetEndDate() =>
			DateTimeOffset.UtcNow.AddDays(-1).Date;

	}
}