using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ReadAssetOperationsUseCase {
		readonly IStateManager _stateManager;

		public ReadAssetOperationsUseCase(IStateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task<IReadOnlyCollection<Operation>> Handle(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId brokerId, AssetId assetId) {
			var state    = await _stateManager.ReadState(endDate, user);
			var broker   = state.Brokers.First(b => b.Id == brokerId);
			var commands = await _stateManager.ReadCommands(startDate, endDate, user);
			var incomeOperations = ReadOperations<AddIncomeModel>(
				commands,
				c => (c.Broker == brokerId) && (c.Asset == assetId),
				c => {
					var account  = broker.Accounts.First(a => a.Id == c.Account);
					var currency = account.Currency;
					var asset    = (c.Asset != null) ? new AssetId(c.Asset) : null;
					return new Operation(c.Date, OperationKind.Income, currency, c.Amount, c.Category, asset);
				});
			var expenseOperations = ReadOperations<AddExpenseModel>(
				commands,
				c => (c.Broker == brokerId) && (c.Asset == assetId),
				c => {
					var account  = broker.Accounts.First(a => a.Id == c.Account);
					var currency = account.Currency;
					var asset    = (c.Asset != null) ? new AssetId(c.Asset) : null;
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