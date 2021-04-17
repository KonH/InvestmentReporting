using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class AddIncomeUseCase {
		readonly IStateManager _stateManager;
		readonly IIdGenerator  _idGenerator;

		public AddIncomeUseCase(IStateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId broker, AccountId account,
			decimal amount, IncomeCategory category, AssetId? asset) {
			if ( amount == 0 ) {
				throw new InvalidPriceException();
			}
			if ( string.IsNullOrWhiteSpace(category.ToString()) ) {
				throw new InvalidCategoryException();
			}
			var state       = _stateManager.ReadState(date, user);
			var brokerState = state.Brokers.FirstOrDefault(b => b.Id == broker);
			if ( brokerState == null ) {
				throw new BrokerNotFoundException();
			}
			if ( brokerState.Accounts.All(a => a.Id != account) ) {
				throw new AccountNotFoundException();
			}
			if ( (asset != null) && brokerState.Inventory.All(a => a.Id != asset) ) {
				throw new AssetNotFoundException();
			}
			var id = new OperationId(_idGenerator.GenerateNewId());
			await _stateManager.AddCommand(new AddIncomeCommand(
				date, user, broker, account, id, amount, category, asset));
		}
	}
}