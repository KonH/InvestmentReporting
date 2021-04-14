using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Logic {
	public sealed class StateManager : IStateManager {
		readonly IStateRepository _repository;

		readonly Dictionary<Type, Func<ICommand, ICommandModel>> _persist = new();
		readonly Dictionary<Type, Func<ICommandModel, ICommand>> _create  = new();
		readonly Dictionary<Type, Action<State, ICommand>>       _apply   = new();

		public StateManager(IStateRepository repository) {
			_repository = repository;
			this.Configure();
		}

		internal void Bind<TCommand, TModel>(
			Func<TCommand, TModel> persist, Func<TModel, TCommand> create, Action<State, TCommand> apply)
			where TCommand : class, ICommand
			where TModel : class, ICommandModel {
			_persist.Add(typeof(TCommand), c => persist((TCommand)c));
			_create.Add(typeof(TModel), m => create((TModel)m));
			_apply.Add(typeof(TCommand), (s, cmd) => apply(s, (TCommand)cmd));
		}

		State Take(DateTimeOffset date, UserId user) {
			var state    = new State(new List<Broker>(), new List<Currency>());
			var commands = this.ReadCommands(date, user);
			foreach ( var command in commands ) {
				var apply = _apply[command.GetType()];
				apply(state, command);
			}
			return state;
		}

		public IReadOnlyDictionary<UserId, ReadOnlyState> ReadStates(DateTimeOffset date) {
			var users = _repository.ReadUsers(date);
			var results = users
				.Select(u => {
					var userId = new UserId(u);
					var state = ReadState(date, userId);
					return (userId, state);
				})
				.ToArray();
			return results
				.ToDictionary(t => t.userId, t => t.state);
		}

		public ReadOnlyState ReadState(DateTimeOffset date, UserId user) =>
			new(Take(date, user));

		public IEnumerable<ICommand> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate) =>
			_repository.ReadCommands(startDate, endDate)
				.Select(model => _create[model.GetType()](model));

		public async Task AddCommand(ICommand command) {
			var model = Persist(command);
			await _repository.SaveCommand(model);
		}

		public async Task ResetCommands(UserId user) {
			var commands = this.ReadCommands(user)
				.Select(Persist)
				.ToArray();
			await _repository.DeleteCommands(commands);
		}

		public async Task ResetOperations(UserId user) {
			var commands = this.ReadCommands(user);
			var filterCommands = commands
				.Where(c => !(c is CreateCurrencyCommand) && !(c is CreateBrokerCommand) && !(c is CreateAccountCommand))
				.Select(Persist)
				.ToArray();
			await _repository.DeleteCommands(filterCommands);
		}

		ICommandModel Persist(ICommand command) =>
			_persist[command.GetType()](command);
	}
}