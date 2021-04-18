using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;

namespace InvestmentReporting.Market.UseCase {
	public sealed class ReadVirtualStateUseCase {
		readonly IStateManager         _stateManager;
		readonly MetadataManager       _metadataManager;
		readonly CurrencyConfiguration _currencyConfig;
		readonly AssetPriceManager     _priceManager;
		readonly ExchangeManager       _exchangeManager;

		public ReadVirtualStateUseCase(
			IStateManager stateManager, MetadataManager metadataManager, CurrencyConfiguration currencyConfig,
			AssetPriceManager priceManager, ExchangeManager exchangeManager) {
			_stateManager    = stateManager;
			_metadataManager = metadataManager;
			_currencyConfig  = currencyConfig;
			_priceManager    = priceManager;
			_exchangeManager = exchangeManager;
		}

		public VirtualState Handle(DateTimeOffset date, UserId user) {
			var state = _stateManager.ReadState(date, user);
			var inventory = state.Brokers
				.SelectMany(broker => broker.Inventory
					.Where(asset => asset.Count > 0)
					.Select(asset => {
						var metadata     = _metadataManager.GetMetadata(asset.Isin);
						var name         = metadata?.Name;
						var type         = metadata?.Type;
						var currency     = asset.Currency;
						var realSum      = _priceManager.GetRealPriceSum(date, user, asset.Id);
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
			var balances = CalculateBalances(inventory, date, user);
			var summary  = CalculateSummary(balances, date);
			return new VirtualState(summary, balances);
		}

		IReadOnlyCollection<VirtualBalance> CalculateBalances(
			IReadOnlyCollection<VirtualAsset> inventory, DateTimeOffset date, UserId user) =>
			_currencyConfig.GetAll()
				.Select(currency => _priceManager.GetVirtualBalance(date, user, currency, inventory))
				.ToArray();

		IReadOnlyDictionary<CurrencyCode, CurrencyBalance> CalculateSummary(
			IReadOnlyCollection<VirtualBalance> balances, DateTimeOffset date) {
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
					return new CurrencyBalance(selfReal + otherReal, selfVirtual + otherVirtual);
				});
		}
	}
}