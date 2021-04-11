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

		async Task<State> Take(DateTimeOffset date, UserId id) {
			var state         = new State(new List<Broker>(), new List<Currency>());
			var commandModels = await _repository.ReadCommands(DateTimeOffset.MinValue, date, id);
			foreach ( var model in commandModels ) {
				var apply = _applies[model.GetType()];
				apply(state, model);
			}
			return state;
		}

		public async Task<IReadOnlyDictionary<UserId, ReadOnlyState>> ReadStates(DateTimeOffset date) {
			var users = await _repository.ReadUsers(date);
			var tasks = users
				.Select(async u => {
					var userId = new UserId(u);
					var state = await ReadState(date, userId);
					return (userId, state);
				})
				.ToArray();
			var results = await Task.WhenAll(tasks);
			return results
				.ToDictionary(t => t.userId, t => t.state);
		}

		public async Task<ReadOnlyState> ReadState(DateTimeOffset date, UserId id) =>
			new(await Take(date, id));

		public async Task<IReadOnlyCollection<ICommandModel>> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId id) =>
			await _repository.ReadCommands(startDate, endDate, id);

		public async Task AddCommand(ICommand command) {
			var model = _persists[command.GetType()](command);
			await _repository.SaveCommand(model);
		}

		public async Task DeleteCommands(IReadOnlyCollection<ICommandModel> commands) {
			await _repository.DeleteCommands(commands);
		}
	}
}