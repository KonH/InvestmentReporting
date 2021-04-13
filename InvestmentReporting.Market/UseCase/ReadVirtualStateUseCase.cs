using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;

namespace InvestmentReporting.Market.UseCase {
	public sealed class ReadVirtualStateUseCase {
		readonly IStateManager   _stateManager;
		readonly MetadataManager _metadataManager;
		readonly PriceManager    _priceManager;

		public ReadVirtualStateUseCase(
			IStateManager stateManager, MetadataManager metadataManager, PriceManager priceManager) {
			_stateManager    = stateManager;
			_metadataManager = metadataManager;
			_priceManager    = priceManager;
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
						var realPrice    = _priceManager.GetRealPriceSum(date, user, asset.Id);
						var virtualPrice = _priceManager.GetVirtualPricePerOne(asset.Isin, date) * asset.Count ?? realPrice;
						return new VirtualAsset(
							asset.Id, broker.Id, asset.Isin, name, type, asset.Count, realPrice, virtualPrice, currency);
					}))
				.ToArray();
			var balances = CalculateBalances(state, inventory, date, user);
			return new VirtualState(balances, inventory);
		}

		IReadOnlyCollection<VirtualBalance> CalculateBalances(
			ReadOnlyState state, IReadOnlyCollection<VirtualAsset> inventory, DateTimeOffset date, UserId user) =>
			state.Currencies
				.Select(currency => _priceManager.GetVirtualBalance(date, user, currency.Id, inventory))
				.ToArray();
	}
}