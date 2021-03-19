using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentReporting.Domain {
	public record UserId(string Value) {
		public override string ToString() => Value;
	}

	public record BrokerId(string Value) {
		public override string ToString() => Value;
	}

	sealed class State {
		public List<BrokerId> Brokers { get; } = new();
	}

	public sealed class ReadOnlyState {
		public IReadOnlyCollection<BrokerId> Brokers { get; }

		internal ReadOnlyState(State state) {
			Brokers = state.Brokers.AsReadOnly();
		}
	}

	public interface IStateCommand {}

	public sealed class OpenBroker : IStateCommand {
		public BrokerId Id { get; }

		public OpenBroker(BrokerId id) {
			Id = id;
		}
	}

	public interface IStateCommandModel {
		public string         Owner { get; }
		public DateTimeOffset Date  { get; }
	}

	public sealed class OpenBrokerModel : IStateCommandModel {
		public string         Owner { get; }
		public DateTimeOffset Date  { get; }
		public string         Id    { get; }

		public OpenBrokerModel(string owner, DateTimeOffset date, string id) {
			Owner = owner;
			Date  = date;
			Id    = id;
		}
	}

	public sealed class StateRepository {
		readonly List<IStateCommandModel> _commands = new();

		public Task<IReadOnlyCollection<IStateCommandModel>> ReadCommands(DateTimeOffset date, UserId id) =>
			Task.FromResult((IReadOnlyCollection<IStateCommandModel>)_commands
				.Where(c => c.Owner == id.Value)
				.Where(c => c.Date <= date)
				.ToArray());

		public Task SaveCommand(IStateCommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}

	public sealed class StateManager {
		readonly StateRepository _repository = new();

		async Task<State> Take(DateTimeOffset date, UserId id) {
			var state = new State();
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				switch ( model ) {
					case OpenBrokerModel openBroker:
						state.Brokers.Add(new(openBroker.Id));
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return state;
		}

		public async Task<ReadOnlyState> Read(DateTimeOffset date, UserId id) =>
			new(await Take(date, id));

		public async Task Push(DateTimeOffset date, UserId id, IStateCommand cmd) {
			var model = cmd switch {
				OpenBroker openBroker => new OpenBrokerModel(id.Value, date, openBroker.Id.Value),
				_ => throw new NotImplementedException()
			};
			await _repository.SaveCommand(model);
		}
	}
}