using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ReadAccountOperationsUseCase {
		readonly IStateManager _stateManager;

		public ReadAccountOperationsUseCase(IStateManager stateManager) {
			_stateManager = stateManager;
		}

		public IReadOnlyCollection<Operation> Handle(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId brokerId, AccountId accountId) {
			var state    = _stateManager.ReadState(endDate, user);
			var broker   = state.Brokers.First(b => b.Id == brokerId);
			var account  = broker.Accounts.First(a => a.Id == accountId);
			var currency = account.Currency;
			var commands = _stateManager.ReadCommands(startDate, endDate, user);
			var incomeOperations = ReadOperations<AddIncomeModel>(
				commands,
				c => (c.Broker == brokerId) && (c.Account == accountId),
				c => {
					var asset = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Income, currency, c.Amount, c.Category, asset);
				});
			var expenseOperations = ReadOperations<AddExpenseModel>(
				commands,
				c => (c.Broker == brokerId) && (c.Account == accountId),
				c => {
					var asset = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Expense, currency, -c.Amount, c.Category, asset);
				});
			return incomeOperations.Concat(expenseOperations)
				.OrderBy(o => o.Date)
				.ToArray();
		}

		IEnumerable<Operation> ReadOperations<T>(
			IReadOnlyCollection<ICommandModel> commands, Func<T, bool> selector, Func<T, Operation> factory) where T : class =>
			commands
				.Select(c => c as T)
				.Where(c => (c != null))
				.Select(c => c!)
				.Where(selector)
				.Select(factory);
	}
}