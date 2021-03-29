using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ReadOperationsUseCase {
		readonly StateManager _stateManager;

		public ReadOperationsUseCase(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task<IReadOnlyCollection<Operation>> Handle(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId user, BrokerId broker, AccountId account) {
			var commands = await _stateManager.ReadCommands(startDate, endDate, user);
			var incomeOperations = ReadOperations<AddIncomeModel>(
				commands,
				c => (c.Broker == broker) && (c.Account == account),
				c => new Operation(c.Date, OperationKind.Income, c.Amount, c.Category));
			var expenseOperations = ReadOperations<AddExpenseModel>(
				commands,
				c => (c.Broker == broker) && (c.Account == account),
				c => new Operation(c.Date, OperationKind.Expense, -c.Amount, c.Category));
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