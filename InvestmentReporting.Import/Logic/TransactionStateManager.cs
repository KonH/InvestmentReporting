using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Import.Logic {
	public sealed class TransactionStateManager : IStateManager {
		readonly StateManager _storeStateManager;

		StateManager? _simulatedStateManager;

		readonly List<ICommand> _commands = new();

		public TransactionStateManager(StateManager storeStateManager) {
			_storeStateManager = storeStateManager;
		}

		public async Task Prepare(UserId id) {
			var commands   = await _storeStateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, id);
			var repository = new InMemoryStateRepository(commands.ToList());
			_simulatedStateManager = new StateManager(repository);
		}

		public Task<ReadOnlyState> ReadState(DateTimeOffset date, UserId id) =>
			_simulatedStateManager!.ReadState(date, id);

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId id) =>
			_simulatedStateManager!.ReadCommands(startDate, endDate, id);

		public async Task AddCommand(ICommand command) {
			await _simulatedStateManager!.AddCommand(command);
			_commands.Add(command);
		}

		public async Task Push() {
			foreach ( var command in _commands ) {
				await _storeStateManager.AddCommand(command);
			}
		}
	}
}