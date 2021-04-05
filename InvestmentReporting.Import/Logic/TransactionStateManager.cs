using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Import.Logic {
	public sealed class TransactionStateManager : IStateManager {
		readonly StateManager _stateManager;

		readonly List<ICommand> _commands = new();

		public TransactionStateManager(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public Task<ReadOnlyState> ReadState(DateTimeOffset date, UserId id) =>
			_stateManager.ReadState(date, id);

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId id) =>
			_stateManager.ReadCommands(startDate, endDate, id);

		public Task AddCommand(ICommand command) {
			_commands.Add(command);
			return Task.CompletedTask;
		}

		public async Task Push() {
			foreach ( var command in _commands ) {
				await _stateManager.AddCommand(command);
			}
		}
	}
}