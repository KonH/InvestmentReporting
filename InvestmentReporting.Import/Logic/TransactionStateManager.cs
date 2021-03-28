using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Import.Logic {
	public sealed class TransactionStateManager {
		readonly StateManager _stateManager;

		readonly List<ICommand> _commands = new();

		public TransactionStateManager(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public Task<ReadOnlyState> Read(DateTimeOffset date, UserId id) =>
			_stateManager.Read(date, id);

		public void Add(ICommand command) {
			_commands.Add(command);
		}

		public async Task Push() {
			foreach ( var command in _commands ) {
				await _stateManager.Push(command);
			}
		}
	}
}