using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.Model;
using InvestmentReporting.Domain.Repository;

namespace InvestmentReporting.Domain {
	namespace Entity {
		public record UserId(string Value) {
			public override string ToString() => Value;
		}

		public record State {}

		public sealed class ReadOnlyState {
			public ReadOnlyState(State state) {}
		}
	}

	namespace Command {
		public interface ICommand {
			DateTimeOffset Date  { get; }
			UserId         Owner { get; }

		}
	}

	namespace Model {
		public interface ICommandModel {
			DateTimeOffset Date  { get; }
			string         Owner { get; }
		}
	}

	namespace Repository {
		public sealed class StateRepository {
			readonly List<ICommandModel> _commands = new();

			public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(DateTimeOffset date, UserId user) =>
				Task.FromResult((IReadOnlyCollection<ICommandModel>)_commands
					.Where(c => c.Date <= date)
					.Where(c => c.Owner == user.ToString())
					.ToArray());

			public Task SaveCommand(ICommandModel model) {
				_commands.Add(model);
				return Task.CompletedTask;
			}
		}
	}

	namespace Logic {
		public sealed class StateManager {
			readonly StateRepository _repository = new();

			async Task<State> Take(DateTimeOffset date, UserId id) {
				var state         = new State();
				var commandModels = await _repository.ReadCommands(date, id);
				foreach ( var model in commandModels ) {
					switch ( model ) {
						default:
							throw new NotSupportedException();
					}
				}
				return state;
			}

			public async Task<ReadOnlyState> Read(DateTimeOffset date, UserId id) =>
				new(await Take(date, id));

			public async Task Push(ICommand command) {
				var model = command switch {
					_  => (ICommandModel)null!
				};
				await _repository.SaveCommand(model);
			}
		}
	}

	namespace UseCase {
		public sealed class ReadStateUseCase {
			readonly StateManager _stateManager;

			public ReadStateUseCase(StateManager stateManager) {
				_stateManager = stateManager;
			}

			public async Task<ReadOnlyState> Handle(DateTimeOffset date, UserId user) => await _stateManager.Read(date, user);
		}
	}
}