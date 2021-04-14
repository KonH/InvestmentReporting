using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Domain.Command;
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
			var incomeOperations = _stateManager.ReadCommands<AddIncomeCommand>(
					startDate, endDate, user, brokerId, accountId)
				.Select(c => {
					var asset = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Income, currency, c.Amount, c.Category, asset);
				});
			var expenseOperations = _stateManager.ReadCommands<AddExpenseCommand>(
					startDate, endDate, user, brokerId, accountId)
				.Select(c => {
					var asset = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Expense, currency, -c.Amount, c.Category, asset);
				});
			return incomeOperations.Concat(expenseOperations)
				.OrderBy(o => o.Date)
				.ToArray();
		}
	}
}