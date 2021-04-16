using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;

namespace InvestmentReporting.Market.UseCase {
	public sealed class ReadVirtualStateUseCase {
		readonly IStateManager     _stateManager;
		readonly MetadataManager   _metadataManager;
		readonly AssetPriceManager _priceManager;
		readonly ExchangeManager   _exchangeManager;

		public ReadVirtualStateUseCase(
			IStateManager stateManager, MetadataManager metadataManager, AssetPriceManager priceManager,
			ExchangeManager exchangeManager) {
			_stateManager    = stateManager;
			_metadataManager = metadataManager;
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
						var currency     = _priceManager.GetCurrency(user, broker.Id, asset.Id);
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
			var balances = CalculateBalances(state, inventory, date, user);
			var summary  = CalculateSummary(balances, date, user);
			return new VirtualState(summary, balances);
		}

		IReadOnlyCollection<VirtualBalance> CalculateBalances(
			ReadOnlyState state, IReadOnlyCollection<VirtualAsset> inventory, DateTimeOffset date, UserId user) =>
			state.Currencies
				.Select(currency => _priceManager.GetVirtualBalance(date, user, currency.Id, inventory))
				.ToArray();

		IReadOnlyDictionary<CurrencyId, CurrencyBalance> CalculateSummary(
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
							otherCurrency, selfCurrency, other.RealSum, date, user);
						otherVirtual += _exchangeManager.Exchange(
							otherCurrency, selfCurrency, other.VirtualSum, date, user);
					}
					return new CurrencyBalance(selfReal + otherReal, selfVirtual + otherVirtual);
				});
		}
	}
}