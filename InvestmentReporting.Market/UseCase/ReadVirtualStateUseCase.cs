using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.UseCase {
	public sealed class ReadVirtualStateUseCase {
		readonly ILogger               _logger;
		readonly IStateManager         _stateManager;
		readonly MetadataManager       _metadataManager;
		readonly CurrencyConfiguration _currencyConfig;
		readonly AssetPriceManager     _priceManager;
		readonly ExchangeManager       _exchangeManager;

		public ReadVirtualStateUseCase(
			ILogger<ReadVirtualStateUseCase> logger, IStateManager stateManager,
			MetadataManager metadataManager, CurrencyConfiguration currencyConfig,
			AssetPriceManager priceManager, ExchangeManager exchangeManager) {
			_logger          = logger;
			_stateManager    = stateManager;
			_metadataManager = metadataManager;
			_currencyConfig  = currencyConfig;
			_priceManager    = priceManager;
			_exchangeManager = exchangeManager;
		}

		public VirtualState Handle(DateTimeOffset date, UserId user, VirtualPeriod period = VirtualPeriod.AllTime) {
			var state       = _stateManager.ReadState(date, user);
			var periodStart = GetPeriodStart(date, period);
			_logger.LogTrace($"Period ({period}) start: {periodStart}");
			var inventory = state.Brokers
				.SelectMany(broker => broker.Inventory
					.Where(asset => asset.Count > 0)
					.Select(asset => {
						var metadata     = _metadataManager.GetMetadataWithFallback(asset.Isin, user);
						var name         = metadata.Name;
						var type         = metadata.Type;
						var currency     = asset.Currency;
						var realSum      = GetRealPriceSum(user, asset, periodStart);
						var realPrice    = realSum / asset.Count;
						var virtualPrice = _priceManager.GetVirtualPricePerOne(asset.Isin, date) ?? realPrice;
						var virtualSum   =  virtualPrice * asset.Count;
						var yearDividend = _priceManager.GetYearDividend(date, user, asset.Id);
						var dividendSum  = _priceManager.GetDividendSum(date, user, asset.Id);
						return new VirtualAsset(
							asset.Id, broker.Id, asset.Isin, name, type, asset.Count,
							realPrice, virtualPrice, realSum, virtualSum,
							yearDividend, dividendSum, currency);
					}))
				.ToArray();
			var balances = CalculateBalances(inventory);
			var summary  = CalculateSummary(balances, date, user);
			return new VirtualState(summary, balances);
		}

		DateTimeOffset GetPeriodStart(DateTimeOffset date, VirtualPeriod period) =>
			period switch {
				VirtualPeriod.AllTime       => DateTimeOffset.MinValue,
				VirtualPeriod.CalendarYear  => date.AddDays(-date.DayOfYear).Date,
				VirtualPeriod.RollingYear   => date.AddDays(-365).Date,
				VirtualPeriod.CalendarMonth => date.AddDays(-date.Day).Date,
				VirtualPeriod.RollingMonth  => date.AddDays(-31).Date,
				VirtualPeriod.CalendarWeek  => date.AddDays(-((int)date.DayOfWeek + 1)).Date,
				VirtualPeriod.RollingWeek   => date.AddDays(-7).Date,
				_                           => throw new ArgumentOutOfRangeException(nameof(period), period, "Unexpected period")
			};

		decimal GetRealPriceSum(UserId user, ReadOnlyAsset asset, DateTimeOffset periodStart) {
			var lastBuyDate = _priceManager.GetLastBuyDate(user, asset.Id);
			_logger.LogTrace($"Last buy date for asset '{asset.Isin}' is {lastBuyDate}");
			if ( lastBuyDate >= periodStart ) {
				_logger.LogTrace($"Last buy date for '{asset.Isin}' inside period, use real price");
				return _priceManager.GetRealPriceSum(lastBuyDate, user, asset.Id);
			}
			_logger.LogTrace($"Last buy date for '{asset.Isin}' inside period, use virtual price at period start");
			var price = _priceManager.GetVirtualPricePerOne(asset.Isin, periodStart);
			if ( price != null ) {
				_logger.LogTrace($"Virtual price for '{asset.Isin}' at period start ('{periodStart}') is {price.Value}'");
				return price.Value * asset.Count;
			}
			_logger.LogWarning($"Failed to find virtual price for '{asset.Isin}', use real one");
			return _priceManager.GetRealPriceSum(DateTimeOffset.MaxValue, user, asset.Id);
		}

		IReadOnlyCollection<VirtualBalance> CalculateBalances(
			IReadOnlyCollection<VirtualAsset> inventory) =>
			_currencyConfig.GetAll()
				.Select(currency => _priceManager.GetVirtualBalance(currency, inventory))
				.ToArray();

		IReadOnlyDictionary<CurrencyCode, CurrencyBalance> CalculateSummary(
			IReadOnlyCollection<VirtualBalance> balances, DateTimeOffset date, UserId user) {
			return balances
				.ToDictionary(b => b.Currency, b => {
					var selfCurrency = b.Currency;
					var selfReal     = b.RealSum;
					var selfVirtual  = b.VirtualSum;
					var otherReal    = 0m;
					var otherVirtual = 0m;
					foreach ( var other in balances ) {
						var otherCurrency = other.Currency;
						if ( otherCurrency == selfCurrency ) {
							continue;
						}
						otherReal += _exchangeManager.Exchange(
							otherCurrency, selfCurrency, other.RealSum, date);
						otherVirtual += _exchangeManager.Exchange(
							otherCurrency, selfCurrency, other.VirtualSum, date);
					}
					var state = _stateManager.ReadState(date, user);
					var accounts = state.Brokers
						.SelectMany(broker => broker.Accounts)
						.Where(a => a.Currency == b.Currency)
						.ToArray();
					var accountSum = accounts.Sum(a => a.Balance);
					return new CurrencyBalance(
						accountSum + selfReal + otherReal,
						accountSum + selfVirtual + otherVirtual);
				});
		}
	}
}