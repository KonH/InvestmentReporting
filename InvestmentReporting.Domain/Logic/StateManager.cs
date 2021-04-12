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

		readonly Dictionary<Type, Func<ICommand, ICommandModel>> _persists = new();
		readonly Dictionary<Type, Action<State, ICommandModel>>  _applies  = new();

		public StateManager(IStateRepository repository) {
			_repository = repository;
			this.Configure();
		}

		internal void Bind<TCommand, TModel>(Func<TCommand, TModel> persist, Action<State, TModel> apply)
			where TCommand : ICommand
			where TModel : ICommandModel {
			_persists.Add(typeof(TCommand), c => persist((TCommand)c));
			_applies.Add(typeof(TModel), (s, m) => apply(s, (TModel)m));
		}

		State Take(DateTimeOffset date, UserId id) {
			var state         = new State(new List<Broker>(), new List<Currency>());
			var commandModels =  _repository.ReadCommands(DateTimeOffset.MinValue, date, id);
			foreach ( var model in commandModels ) {
				var apply = _applies[model.GetType()];
				apply(state, model);
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

		public ReadOnlyState ReadState(DateTimeOffset date, UserId id) =>
			new(Take(date, id));

		public IReadOnlyCollection<ICommandModel> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate) =>
			_repository.ReadCommands(startDate, endDate);

		public IReadOnlyCollection<ICommandModel> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId id) =>
			_repository.ReadCommands(startDate, endDate, id);

		public async Task AddCommand(ICommand command) {
			var model = _persists[command.GetType()](command);
			await _repository.SaveCommand(model);
		}

		public async Task DeleteCommands(IReadOnlyCollection<ICommandModel> commands) {
			await _repository.DeleteCommands(commands);
		}
	}
}