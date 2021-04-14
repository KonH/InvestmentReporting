using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ReadAssetOperationsUseCase {
		readonly IStateManager _stateManager;

		public ReadAssetOperationsUseCase(IStateManager stateManager) {
			_stateManager = stateManager;
		}

		public IReadOnlyCollection<Operation> Handle(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId brokerId, AssetId assetId) {
			var state    = _stateManager.ReadState(endDate, user);
			var broker   = state.Brokers.First(b => b.Id == brokerId);
			var incomeOperations = _stateManager.ReadCommands<AddIncomeCommand>(
					startDate, endDate, user, brokerId, assetId)
				.Select(c => {
					var account  = broker.Accounts.First(a => a.Id == c.Account);
					var currency = account.Currency;
					var asset    = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Income, currency, c.Amount, c.Category, asset);
				});
			var expenseOperations = _stateManager.ReadCommands<AddExpenseCommand>(
				startDate, endDate, user, brokerId, assetId)
				.Select(c => {
					var account  = broker.Accounts.First(a => a.Id == c.Account);
					var currency = account.Currency;
					var asset    = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Expense, currency, -c.Amount, c.Category, asset);
				});
			return incomeOperations.Concat(expenseOperations)
				.OrderBy(o => o.Date)
				.ToArray();
		}
	}
}