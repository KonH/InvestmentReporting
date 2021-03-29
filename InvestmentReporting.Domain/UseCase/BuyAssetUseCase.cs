using System;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class BuyAssetUseCase {
		readonly StateManager _stateManager;
		readonly IIdGenerator _idGenerator;

		public BuyAssetUseCase(StateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId broker,
			string name, AssetCategory category, AssetTicker ticker, CurrencyId currency, decimal price, int count) {
			var id = new AssetId(_idGenerator.GenerateNewId());
			await _stateManager.PushCommand(new AddAssetCommand(date, user, broker, id, name, category, ticker, currency, price, count));
		}
	}
}