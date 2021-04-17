using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Import.Logic {
	public sealed class TransactionStateManager : IStateManager {
		readonly ILoggerFactory _loggerFactory;
		readonly ILogger        _logger;
		readonly StateManager   _storeStateManager;

		StateManager? _simulatedStateManager;

		readonly List<ICommand> _commands = new();

		public TransactionStateManager(ILoggerFactory loggerFactory, StateManager storeStateManager) {
			_loggerFactory     = loggerFactory;
			_logger            = loggerFactory.CreateLogger<TransactionStateManager>();
			_storeStateManager = storeStateManager;
		}

		public void Prepare(UserId user) {
			_logger.LogTrace($"Prepare for '{user}'");
			var repository = new InMemoryStateRepository(_loggerFactory.CreateLogger<InMemoryStateRepository>(), new());
			_simulatedStateManager = new StateManager(repository);
			var commands = _storeStateManager.ReadCommands(user);
			foreach ( var command in commands ) {
				_simulatedStateManager.AddCommand(command);
			}
		}

		public IReadOnlyDictionary<UserId, ReadOnlyState> ReadStates(DateTimeOffset date) {
			return (_simulatedStateManager != null)
				? _simulatedStateManager.ReadStates(date)
				: new Dictionary<UserId, ReadOnlyState>();
		}

		public ReadOnlyState ReadState(DateTimeOffset date, UserId id) =>
			_simulatedStateManager!.ReadState(date, id);

		public IEnumerable<ICommand> ReadCommands(DateTimeOffset startDate, DateTimeOffset endDate) =>
			_simulatedStateManager!.ReadCommands(startDate, endDate);

		public async Task AddCommand(ICommand command) {
			await _simulatedStateManager!.AddCommand(command);
			_commands.Add(command);
		}

		public async Task Push() {
			_logger.LogTrace("Pushing in memory commands started");
			foreach ( var command in _commands ) {
				await _storeStateManager.AddCommand(command);
			}
			_logger.LogTrace("Pushing in memory commands finished");
		}
	}
}