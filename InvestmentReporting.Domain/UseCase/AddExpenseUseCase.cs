using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class AddExpenseUseCase {
		readonly StateManager _stateManager;
		readonly IIdGenerator _idGenerator;

		public AddExpenseUseCase(StateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(
			DateTimeOffset date, UserId user, BrokerId broker, AccountId account,
			CurrencyId currency, decimal amount, decimal exchangeRate, ExpenseCategory category) {
			if ( amount == 0 ) {
				throw new InvalidPriceException();
			}
			if ( exchangeRate == 0 ) {
				throw new InvalidPriceException();
			}
			if ( string.IsNullOrWhiteSpace(category.ToString()) ) {
				throw new InvalidCategoryException();
			}
			var state = await _stateManager.Read(date, user);
			if ( state.Currencies.All(c => c.Id != currency) ) {
				throw new CurrencyNotFoundException();
			}
			var brokerState = state.Brokers.FirstOrDefault(b => b.Id == broker);
			if ( brokerState == null ) {
				throw new BrokerNotFoundException();
			}
			if ( brokerState.Accounts.All(a => a.Id != account) ) {
				throw new AccountNotFoundException();
			}
			var id = new OperationId(_idGenerator.GenerateNewId());
			await _stateManager.Push(new AddExpenseCommand(
				date, user, broker, account, id, currency, amount, exchangeRate, category));
		}
	}
}