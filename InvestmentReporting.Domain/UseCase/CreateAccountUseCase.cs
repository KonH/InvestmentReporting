using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class CreateAccountUseCase {
		readonly StateManager _stateManager;
		readonly IIdGenerator _idGenerator;

		public CreateAccountUseCase(StateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId broker, CurrencyId currency, string displayName) {
			if ( string.IsNullOrWhiteSpace(displayName) ) {
				throw new InvalidAccountException();
			}
			var state = await _stateManager.ReadState(date, user);
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
			await _stateManager.PushCommand(new CreateAccountCommand(date, user, broker, id, currency, displayName));
		}
	}
}