using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class CreateAccountUseCase {
		readonly IStateManager _stateManager;
		readonly IIdGenerator  _idGenerator;

		public CreateAccountUseCase(IStateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId broker, CurrencyId currency, string displayName) {
			if ( string.IsNullOrWhiteSpace(displayName) ) {
				throw new InvalidAccountException();
			}
			var state = _stateManager.ReadState(date, user);
			if ( state.Currencies.All(c => c.Id != currency) ) {
				throw new CurrencyNotFoundException();
			}
			var brokerState = state.Brokers.FirstOrDefault(b => b.Id == broker);
			if ( brokerState == null ) {
				throw new BrokerNotFoundException();
			}
			if ( brokerState.Accounts.Any(a => a.DisplayName == displayName) ) {
				throw new DuplicateAccountException();
			}
			var id = new AccountId(_idGenerator.GenerateNewId());
			await _stateManager.AddCommand(new CreateAccountCommand(date, user, broker, id, currency, displayName));
		}
	}
}