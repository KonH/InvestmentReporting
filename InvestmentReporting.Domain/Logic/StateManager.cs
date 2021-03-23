using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Logic {
	public sealed class StateManager {
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
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				var apply = _applies[model.GetType()];
				apply(state, model);
			}
			return state;
		}

		public async Task<ReadOnlyState> Read(DateTimeOffset date, UserId id) =>
			new(await Take(date, id));

		public async Task Push(ICommand command) {
			var model = _persists[command.GetType()](command);
			await _repository.SaveCommand(model);
		}
	}
}