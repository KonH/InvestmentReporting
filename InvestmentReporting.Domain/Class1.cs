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

	sealed class StateEntity {
		public List<BrokerId> Brokers { get; } = new();
	}

	public sealed class ReadOnlyState {
		public IReadOnlyCollection<BrokerId> Brokers { get; }

		internal ReadOnlyState(StateEntity state) {
			Brokers = state.Brokers.AsReadOnly();
		}
	}

	public sealed class BrokerEntity {
		public UserId Owner       { get; }
		public string DisplayName { get; }
		public bool   IsOpen      { get; set; }

		public BrokerEntity(UserId owner, string displayName) {
			Owner       = owner;
			DisplayName = displayName;
		}
	}

	public sealed class ReadOnlyBroker {
		public string DisplayName { get; }
		public bool   IsOpen      { get; }

		internal ReadOnlyBroker(BrokerEntity broker) {
			DisplayName = broker.DisplayName;
			IsOpen      = broker.IsOpen;
		}
	}

	public interface IStateCommand {}

	public sealed class CreateState : IStateCommand {}

	public sealed class CreateBroker : IStateCommand {
		public readonly UserId User;
		public readonly string DisplayName;

		public CreateBroker(UserId user, string displayName) {
			User        = user;
			DisplayName = displayName;
		}
	}

	public sealed class OpenBroker : IStateCommand {
		public BrokerId Id { get; }

		public OpenBroker(BrokerId id) {
			Id = id;
		}
	}

	public interface ICommandModel {
		public string         Owner { get; }
		public DateTimeOffset Date  { get; }
	}

	namespace State.Models {
		public sealed class CreateStateModel : ICommandModel {
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }

			public CreateStateModel(UserId owner, DateTimeOffset date) {
				Owner = owner.Value;
				Date  = date;
			}
		}

		public sealed class OpenBrokerModel : ICommandModel {
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }
			public string         Id    { get; }

			public OpenBrokerModel(UserId owner, DateTimeOffset date, BrokerId id) {
				Owner = owner.Value;
				Date  = date;
				Id    = id.Value;
			}
		}
	}

	namespace Broker.Models {
		public sealed class CreateBrokerModel : ICommandModel {
			public string         Owner       { get; }
			public DateTimeOffset Date        { get; }
			public string         User        { get; }
			public string         DisplayName { get; }

			public CreateBrokerModel(BrokerId owner, DateTimeOffset date, UserId user, string displayName) {
				Owner       = owner.Value;
				Date        = date;
				User        = user.Value;
				DisplayName = displayName;
			}
		}

		public sealed class OpenBrokerModel : ICommandModel {
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }

			public OpenBrokerModel(BrokerId owner, DateTimeOffset date) {
				Owner = owner.Value;
				Date  = date;
			}
		}
	}

	public sealed class StateRepository {
		readonly List<ICommandModel> _commands = new();

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(DateTimeOffset date, UserId id) =>
			Task.FromResult((IReadOnlyCollection<ICommandModel>)_commands
				.Where(c => c.Owner == id.Value)
				.Where(c => c.Date <= date)
				.ToArray());

		public Task SaveCommand(ICommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}

	public sealed class StateManager {
		readonly StateRepository _repository = new();

		async Task<StateEntity?> Take(DateTimeOffset date, UserId id) {
			StateEntity? state = null;
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				switch ( model ) {
					case State.Models.CreateStateModel:
						state = new StateEntity();
						break;
					case State.Models.OpenBrokerModel openBroker:
						state?.Brokers.Add(new(openBroker.Id));
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return state;
		}

		internal async Task<ReadOnlyState?> Read(DateTimeOffset date, UserId id) {
			var state = await Take(date, id);
			return (state != null) ? new(state) : null;
		}

		internal async Task Push(DateTimeOffset date, UserId id, IStateCommand cmd) {
			ICommandModel? model = cmd switch {
				CreateState           => new State.Models.CreateStateModel(id, date),
				OpenBroker openBroker => new State.Models.OpenBrokerModel(id, date, openBroker.Id),
				_                     => null
			};
			if ( model != null ) {
				await _repository.SaveCommand(model);
			}
		}
	}

	public sealed class BrokerRepository {
		readonly List<ICommandModel> _commands = new();

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(DateTimeOffset date, BrokerId id) =>
			Task.FromResult((IReadOnlyCollection<ICommandModel>)_commands
				.Where(c => c.Owner == id.Value)
				.Where(c => c.Date <= date)
				.ToArray());

		public Task SaveCommand(ICommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}

	public sealed class BrokerManager {
		readonly BrokerRepository _repository = new();

		async Task<BrokerEntity?> Take(DateTimeOffset date, BrokerId id) {
			BrokerEntity? broker = null;
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				switch ( model ) {
					case Broker.Models.CreateBrokerModel createBroker:
						broker = new BrokerEntity(new(createBroker.Owner), createBroker.DisplayName);
						break;
					case Broker.Models.OpenBrokerModel:
						if ( broker != null ) {
							broker.IsOpen = true;
						}
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return broker;
		}

		internal async Task<ReadOnlyBroker?> Read(DateTimeOffset date, BrokerId id) {
			var state = await Take(date, id);
			return (state != null) ? new(state) : null;
		}

		internal async Task Push(DateTimeOffset date, BrokerId id, IStateCommand cmd) {
			ICommandModel? model = cmd switch {
				CreateBroker createBroker => new Broker.Models.CreateBrokerModel(id, date, createBroker.User, createBroker.DisplayName),
				OpenBroker _              => new Broker.Models.OpenBrokerModel(id, date),
				_                         => null
			};
			if ( model != null ) {
				await _repository.SaveCommand(model);
			}
		}
	}

	public sealed class IdGenerator {
		public string GenerateNewId() => Guid.NewGuid().ToString();
	}

	public sealed class OpenBrokerUseCase {
		readonly StateManager  _stateManager;
		readonly BrokerManager _brokerManager;
		readonly IdGenerator   _idGenerator;

		public OpenBrokerUseCase(StateManager stateManager, BrokerManager brokerManager, IdGenerator idGenerator) {
			_stateManager  = stateManager;
			_brokerManager = brokerManager;
			_idGenerator   = idGenerator;
		}

		public async Task Handle(DateTimeOffset date, UserId user, string displayName) {
			var state = await _stateManager.Read(date, user);
			if ( state == null ) {
				await _stateManager.Push(date, user, new CreateState());
			}
			var id = new BrokerId(_idGenerator.GenerateNewId());
			await _brokerManager.Push(date, id, new CreateBroker(user, displayName));
			await _brokerManager.Push(date, id, new OpenBroker(id));
			await _stateManager.Push(date, user, new OpenBroker(id));
		}
	}

	public sealed class ReadBrokersUseCase {
		readonly StateManager  _stateManager;
		readonly BrokerManager _brokerManager;

		public ReadBrokersUseCase(StateManager stateManager, BrokerManager brokerManager) {
			_stateManager  = stateManager;
			_brokerManager = brokerManager;
		}

		public async Task<IReadOnlyCollection<ReadOnlyBroker>> Handle(DateTimeOffset date, UserId user) {
			var state = await _stateManager.Read(date, user);
			if ( state == null ) {
				return Array.Empty<ReadOnlyBroker>();
			}
			var brokerIds = state.Brokers;
			var brokerTasks = brokerIds
				.Select(id => _brokerManager.Read(date, id));
			var brokers = await Task.WhenAll(brokerTasks);
			return brokers
				.Where(b => b?.IsOpen ?? false)
				.Select(b => b!)
				.ToArray();
		}
	}
}