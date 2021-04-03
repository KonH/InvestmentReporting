using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class AddIncomeUseCase {
		readonly StateManager _stateManager;
		readonly IIdGenerator _idGenerator;

		public AddIncomeUseCase(StateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId broker, AccountId account,
			decimal amount, IncomeCategory category) {
			if ( amount == 0 ) {
				throw new InvalidPriceException();
			}
			if ( string.IsNullOrWhiteSpace(category.ToString()) ) {
				throw new InvalidCategoryException();
			}
			var state       = await _stateManager.ReadState(date, user);
			var brokerState = state.Brokers.FirstOrDefault(b => b.Id == broker);
			if ( brokerState == null ) {
				throw new BrokerNotFoundException();
			}
			if ( brokerState.Accounts.All(a => a.Id != account) ) {
				throw new AccountNotFoundException();
			}
			var id = new OperationId(_idGenerator.GenerateNewId());
			await _stateManager.PushCommand(new AddIncomeCommand(
				date, user, broker, account, id, amount, category));
		}
	}
}