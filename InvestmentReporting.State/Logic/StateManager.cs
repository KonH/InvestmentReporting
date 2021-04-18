using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Logic {
	public sealed class StateManager : IStateManager {
		readonly IStateRepository _repository;

		readonly Dictionary<Type, Func<ICommand, ICommandModel>>  _persist = new();
		readonly Dictionary<Type, Func<ICommandModel, ICommand>>  _create  = new();
		readonly Dictionary<Type, Action<Entity.State, ICommand>> _apply   = new();

		public StateManager(IStateRepository repository) {
			_repository = repository;
			this.Configure();
		}

		internal void Bind<TCommand, TModel>(
			Func<TCommand, TModel> persist, Func<TModel, TCommand> create, Action<Entity.State, TCommand> apply)
			where TCommand : class, ICommand
			where TModel : class, ICommandModel {
			_persist.Add(typeof(TCommand), c => persist((TCommand)c));
			_create.Add(typeof(TModel), m => create((TModel)m));
			_apply.Add(typeof(TCommand), (s, cmd) => apply(s, (TCommand)cmd));
		}

		Entity.State Take(DateTimeOffset date, UserId user) {
			var state    = new Entity.State(new List<Broker>());
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
				.Where(c => c is not CreateBrokerCommand && c is not CreateAccountCommand)
				.Select(Persist)
				.ToArray();
			await _repository.DeleteCommands(filterCommands);
		}

		ICommandModel Persist(ICommand command) =>
			_persist[command.GetType()](command);
	}
}