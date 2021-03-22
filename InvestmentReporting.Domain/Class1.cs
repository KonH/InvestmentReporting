using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentReporting.Domain {
	namespace Shared.Entity {
		public record UserId(string Value) {
			public override string ToString() => Value;
		}

		public record StateId(string Value) {
			public override string ToString() => Value;
		}

		public record BrokerId(string Value) {
			public override string ToString() => Value;
		}

		public record CurrencyId(string Value) {
			public override string ToString() => Value;
		}

		public record AccountId(string Value) {
			public override string ToString() => Value;
		}
	}

	namespace State.Entity {
		sealed class StateEntity {
			public Shared.Entity.StateId          Id         { get; }
			public List<Shared.Entity.BrokerId>   Brokers    { get; } = new();
			public List<Shared.Entity.CurrencyId> Currencies { get; } = new();

			public StateEntity(Shared.Entity.StateId id) {
				Id = id;
			}
		}

		public sealed class ReadOnlyStateEntity {
			public IReadOnlyCollection<Shared.Entity.BrokerId>   Brokers    { get; }
			public IReadOnlyCollection<Shared.Entity.CurrencyId> Currencies { get; }

			internal ReadOnlyStateEntity(StateEntity state) {
				Brokers    = state.Brokers.AsReadOnly();
				Currencies = state.Currencies.AsReadOnly();
			}
		}
	}

	namespace Broker.Entity {
		public sealed class BrokerEntity {
			public Shared.Entity.UserId Owner       { get; }
			public string               DisplayName { get; }
			public bool                 IsOpen      { get; set; }

			public BrokerEntity(Shared.Entity.UserId owner, string displayName) {
				Owner       = owner;
				DisplayName = displayName;
			}
		}

		public sealed class ReadOnlyBrokerEntity {
			public string DisplayName { get; }
			public bool   IsOpen      { get; }

			internal ReadOnlyBrokerEntity(BrokerEntity broker) {
				DisplayName = broker.DisplayName;
				IsOpen      = broker.IsOpen;
			}
		}
	}

	namespace Shared.Commands {
		public interface ICommand {}
	}

	namespace State.Commands {
		public interface IStateCommand : Shared.Commands.ICommand {}

		public sealed class CreateState : IStateCommand {}

		public sealed class OpenBroker : IStateCommand {
			public Shared.Entity.BrokerId Id { get; }

			public OpenBroker(Shared.Entity.BrokerId id) {
				Id = id;
			}
		}

		public sealed class AddCurrency : IStateCommand {
			public Shared.Entity.CurrencyId Id { get; }

			public AddCurrency(Shared.Entity.CurrencyId id) {
				Id = id;
			}
		}
	}

	namespace Broker.Commands {
		public interface IBrokerCommand : Shared.Commands.ICommand {}

		public sealed class CreateBroker : IBrokerCommand {
			public readonly Shared.Entity.StateId Owner;
			public readonly string DisplayName;

			public CreateBroker(Shared.Entity.StateId owner, string displayName) {
				Owner       = owner;
				DisplayName = displayName;
			}
		}

		public sealed class OpenBroker : IBrokerCommand {}
	}

	namespace Currency.Commands {
		public interface ICurrencyCommand : Shared.Commands.ICommand {}

		public sealed class CreateCurrency : ICurrencyCommand {
			public readonly string Code;
			public readonly string Format;

			public CreateCurrency(string code, string format) {
				Code   = code;
				Format = format;
			}
		}
	}

	namespace Shared.Models {
		public interface ICommandModel {
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }
		}
	}

	namespace State.Models {
		public interface IStateCommandModel : Shared.Models.ICommandModel {
			string User { get; }
		}

		public sealed class CreateStateModel : IStateCommandModel {
			public string         User  { get; }
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }

			public CreateStateModel(Shared.Entity.UserId user, Shared.Entity.StateId owner, DateTimeOffset date) {
				User  = user.Value;
				Owner = owner.Value;
				Date  = date;
			}
		}

		public sealed class OpenBrokerModel : IStateCommandModel {
			public string         User  { get; }
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }
			public string         Id    { get; }

			public OpenBrokerModel(Shared.Entity.UserId user, Shared.Entity.StateId owner, DateTimeOffset date, Shared.Entity.BrokerId id) {
				User  = user.Value;
				Owner = owner.Value;
				Date  = date;
				Id    = id.Value;
			}
		}

		public sealed class AddCurrencyModel : IStateCommandModel {
			public string         User  { get; }
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }
			public string         Id    { get; }

			public AddCurrencyModel(Shared.Entity.UserId user, Shared.Entity.StateId owner, DateTimeOffset date, Shared.Entity.CurrencyId id) {
				User  = user.Value;
				Owner = owner.Value;
				Date  = date;
				Id    = id.Value;
			}
		}
	}

	namespace Broker.Models {
		public interface IBrokerCommandModel : Shared.Models.ICommandModel {}

		public sealed class CreateBrokerModel : IBrokerCommandModel {
			public string         Owner       { get; }
			public DateTimeOffset Date        { get; }
			public string         State       { get; }
			public string         DisplayName { get; }

			public CreateBrokerModel(Shared.Entity.BrokerId owner, DateTimeOffset date, Shared.Entity.StateId state, string displayName) {
				Owner       = owner.Value;
				Date        = date;
				State       = state.Value;
				DisplayName = displayName;
			}
		}

		public sealed class OpenBrokerModel : IBrokerCommandModel {
			public string         Owner { get; }
			public DateTimeOffset Date  { get; }

			public OpenBrokerModel(Shared.Entity.BrokerId owner, DateTimeOffset date) {
				Owner = owner.Value;
				Date  = date;
			}
		}
	}

	namespace Currency.Models {
		public interface ICurrencyCommandModel : Shared.Models.ICommandModel {}

		public sealed class CreateCurrencyModel : ICurrencyCommandModel {
			public string         Owner  { get; }
			public DateTimeOffset Date   { get; }
			public string         Code   { get; }
			public string         Format { get; }

			public CreateCurrencyModel(Shared.Entity.CurrencyId owner, DateTimeOffset date, string code, string format) {
				Owner  = owner.Value;
				Date   = date;
				Code   = code;
				Format = format;
			}
		}
	}

	public sealed class StateRepository {
		readonly List<State.Models.IStateCommandModel> _commands = new();

		public Task<IReadOnlyCollection<State.Models.IStateCommandModel>> ReadCommands(DateTimeOffset date, Shared.Entity.StateId id) =>
			Task.FromResult((IReadOnlyCollection<State.Models.IStateCommandModel>)_commands
				.Where(c => c.Owner == id.Value)
				.Where(c => c.Date <= date)
				.ToArray());

		public Task<IReadOnlyCollection<State.Models.IStateCommandModel>> ReadCommands(Shared.Entity.UserId id) =>
			Task.FromResult((IReadOnlyCollection<State.Models.IStateCommandModel>)_commands
				.Where(c => c.User == id.Value)
				.ToArray());

		public Task SaveCommand(State.Models.IStateCommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}

	public sealed class StateManager {
		readonly StateRepository _repository = new();

		async Task<State.Entity.StateEntity?> Take(DateTimeOffset date, Shared.Entity.StateId id) {
			State.Entity.StateEntity? state = null;
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				switch ( model ) {
					case State.Models.CreateStateModel create:
						state = new State.Entity.StateEntity(new Shared.Entity.StateId(create.Owner));
						break;
					case State.Models.OpenBrokerModel openBroker:
						state?.Brokers.Add(new(openBroker.Id));
						break;
					case State.Models.AddCurrencyModel addCurrency:
						state?.Currencies.Add(new(addCurrency.Id));
						break;
					default:
						throw new InvalidOperationException(model.GetType().FullName);
				}
			}
			return state;
		}

		internal async Task<Shared.Entity.StateId?> ReadLatestStateIdForUser(Shared.Entity.UserId user) {
			var commands = await _repository.ReadCommands(user);
			var createCommand = commands
				.Where(c => c is State.Models.CreateStateModel)
				.Cast<State.Models.CreateStateModel>()
				.LastOrDefault();
			return (createCommand != null) ? new(createCommand.Owner) : null;
		}

		internal async Task<State.Entity.ReadOnlyStateEntity?> Read(DateTimeOffset date, Shared.Entity.StateId id) {
			var state = await Take(date, id);
			return (state != null) ? new(state) : null;
		}

		internal async Task Push(DateTimeOffset date, Shared.Entity.UserId user, Shared.Entity.StateId owner, State.Commands.IStateCommand cmd) {
			State.Models.IStateCommandModel? model = cmd switch {
				State.Commands.CreateState    => new State.Models.CreateStateModel(user, owner, date),
				State.Commands.OpenBroker openBroker => new State.Models.OpenBrokerModel(user, owner, date, openBroker.Id),
				State.Commands.AddCurrency addCurrency => new State.Models.AddCurrencyModel(user, owner, date, addCurrency.Id),
				_                     => null
			};
			if ( model != null ) {
				await _repository.SaveCommand(model);
			}
		}
	}

	public sealed class BrokerRepository {
		readonly List<Broker.Models.IBrokerCommandModel> _commands = new();

		public Task<IReadOnlyCollection<Broker.Models.IBrokerCommandModel>> ReadCommands(DateTimeOffset date, Shared.Entity.BrokerId id) =>
			Task.FromResult((IReadOnlyCollection<Broker.Models.IBrokerCommandModel>)_commands
				.Where(c => c.Owner == id.Value)
				.Where(c => c.Date <= date)
				.ToArray());

		public Task SaveCommand(Broker.Models.IBrokerCommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}

	public sealed class BrokerManager {
		readonly BrokerRepository _repository = new();

		async Task<Broker.Entity.BrokerEntity?> Take(DateTimeOffset date, Shared.Entity.BrokerId id) {
			Broker.Entity.BrokerEntity? broker = null;
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				switch ( model ) {
					case Broker.Models.CreateBrokerModel createBroker:
						broker = new Broker.Entity.BrokerEntity(new(createBroker.Owner), createBroker.DisplayName);
						break;
					case Broker.Models.OpenBrokerModel:
						if ( broker != null ) {
							broker.IsOpen = true;
						}
						break;
					default:
						throw new InvalidOperationException(model.GetType().FullName);
				}
			}
			return broker;
		}

		internal async Task<Broker.Entity.ReadOnlyBrokerEntity?> Read(DateTimeOffset date, Shared.Entity.BrokerId id) {
			var state = await Take(date, id);
			return (state != null) ? new(state) : null;
		}

		internal async Task Push(DateTimeOffset date, Shared.Entity.BrokerId id, Broker.Commands.IBrokerCommand cmd) {
			Broker.Models.IBrokerCommandModel? model = cmd switch {
				Broker.Commands.CreateBroker createBroker => new Broker.Models.CreateBrokerModel(id, date, createBroker.Owner, createBroker.DisplayName),
				Broker.Commands.OpenBroker _              => new Broker.Models.OpenBrokerModel(id, date),
				_                         => null
			};
			if ( model != null ) {
				await _repository.SaveCommand(model);
			}
		}
	}

	namespace Currency.Entity {
		sealed class CurrencyEntity {
			public string Code   { get; }
			public string Format { get; }

			public CurrencyEntity(string code, string format) {
				Code   = code;
				Format = format;
			}
		}

		public sealed class ReadOnlyCurrencyEntity {
			public string Code   { get; }
			public string Format { get; }

			internal ReadOnlyCurrencyEntity(CurrencyEntity currency) {
				Code   = currency.Code;
				Format = currency.Format;
			}
		}
	}

	public sealed class CurrencyRepository {
		readonly List<Currency.Models.ICurrencyCommandModel> _commands = new();

		public Task<IReadOnlyCollection<Currency.Models.ICurrencyCommandModel>> ReadCommands(DateTimeOffset date, Shared.Entity.CurrencyId id) =>
			Task.FromResult((IReadOnlyCollection<Currency.Models.ICurrencyCommandModel>)_commands
				.Where(c => c.Owner == id.Value)
				.Where(c => c.Date <= date)
				.ToArray());

		public Task SaveCommand(Currency.Models.ICurrencyCommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}

	public sealed class CurrencyManager {
		readonly CurrencyRepository _repository = new();

		async Task<Currency.Entity.CurrencyEntity?> Take(DateTimeOffset date, Shared.Entity.CurrencyId id) {
			Currency.Entity.CurrencyEntity? state = null;
			var commandModels = await _repository.ReadCommands(date, id);
			foreach ( var model in commandModels ) {
				switch ( model ) {
					case Currency.Models.CreateCurrencyModel createModel:
						state = new Currency.Entity.CurrencyEntity(createModel.Code, createModel.Format);
						break;
					default:
						throw new InvalidOperationException(model.GetType().FullName);
				}
			}
			return state;
		}

		internal async Task<Currency.Entity.ReadOnlyCurrencyEntity?> Read(DateTimeOffset date, Shared.Entity.CurrencyId id) {
			var state = await Take(date, id);
			return (state != null) ? new(state) : null;
		}

		internal async Task Push(DateTimeOffset date, Shared.Entity.CurrencyId id, Currency.Commands.ICurrencyCommand cmd) {
			Currency.Models.ICurrencyCommandModel? model = cmd switch {
				Currency.Commands.CreateCurrency createCurrency => new Currency.Models.CreateCurrencyModel(id, date, createCurrency.Code, createCurrency.Format),
				_              => null
			};
			if ( model != null ) {
				await _repository.SaveCommand(model);
			}
		}
	}

	namespace Account.Entity {
		sealed class AccountEntity {
			public decimal Sum { get; set; }
		}

		public sealed class ReadOnlyAccountEntity {
			public decimal Sum { get; }

			internal ReadOnlyAccountEntity(AccountEntity account) {
				Sum = account.Sum;
			}
		}
	}

	public sealed class IdGenerator {
		public string GenerateNewId() => Guid.NewGuid().ToString();
	}

	public sealed class EnsureStateUseCase {
		readonly StateManager _stateManager;
		readonly IdGenerator  _idGenerator;

		public EnsureStateUseCase(StateManager stateManager, IdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task<Shared.Entity.StateId> Handle(DateTimeOffset date, Shared.Entity.UserId user) {
			var stateId = await _stateManager.ReadLatestStateIdForUser(user);
			if ( stateId != null ) {
				return stateId;
			}
			stateId = new Shared.Entity.StateId(_idGenerator.GenerateNewId());
			await _stateManager.Push(date, user, stateId, new State.Commands.CreateState());
			return stateId;
		}
	}

	public sealed class ReadStateUseCase {
		readonly StateManager _stateManager;

		public ReadStateUseCase(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task<State.Entity.ReadOnlyStateEntity?> Handle(DateTimeOffset date, Shared.Entity.StateId state) {
			return await _stateManager.Read(date, state);
		}
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

		public async Task Handle(DateTimeOffset date, Shared.Entity.UserId user, Shared.Entity.StateId stateId, string displayName) {
			var state = await _stateManager.Read(date, stateId);
			if ( state == null ) {
				await _stateManager.Push(date, user, stateId, new State.Commands.CreateState());
			}
			var brokerId = new Shared.Entity.BrokerId(_idGenerator.GenerateNewId());
			await _brokerManager.Push(date, brokerId, new Broker.Commands.CreateBroker(stateId, displayName));
			await _brokerManager.Push(date, brokerId, new Broker.Commands.OpenBroker());
			await _stateManager.Push(date, user, stateId, new State.Commands.OpenBroker(brokerId));
		}
	}

	public sealed class ReadBrokersUseCase {
		readonly StateManager  _stateManager;
		readonly BrokerManager _brokerManager;

		public ReadBrokersUseCase(StateManager stateManager, BrokerManager brokerManager) {
			_stateManager  = stateManager;
			_brokerManager = brokerManager;
		}

		public async Task<IReadOnlyCollection<Broker.Entity.ReadOnlyBrokerEntity>> Handle(DateTimeOffset date, Shared.Entity.StateId stateId) {
			var state = await _stateManager.Read(date, stateId);
			if ( state == null ) {
				return Array.Empty<Broker.Entity.ReadOnlyBrokerEntity>();
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

	public sealed class CreateCurrencyUseCase {
		readonly StateManager    _stateManager;
		readonly CurrencyManager _currencyManager;
		readonly IdGenerator     _idGenerator;

		public CreateCurrencyUseCase(StateManager stateManager, CurrencyManager currencyManager, IdGenerator idGenerator) {
			_stateManager    = stateManager;
			_currencyManager = currencyManager;
			_idGenerator     = idGenerator;
		}

		public async Task Handle(DateTimeOffset date, Shared.Entity.UserId user, Shared.Entity.StateId stateId, string code, string format) {
			var id = new Shared.Entity.CurrencyId(_idGenerator.GenerateNewId());
			await _currencyManager.Push(date, id, new Currency.Commands.CreateCurrency(code, format));
			await _stateManager.Push(date, user, stateId, new State.Commands.AddCurrency(id));
		}
	}

	public sealed class ReadCurrenciesUseCase {
		readonly StateManager    _stateManager;
		readonly CurrencyManager _currencyManager;

		public ReadCurrenciesUseCase(StateManager stateManager, CurrencyManager currencyManager) {
			_stateManager    = stateManager;
			_currencyManager = currencyManager;
		}

		public async Task<IReadOnlyCollection<Currency.Entity.ReadOnlyCurrencyEntity>> Handle(DateTimeOffset date, Shared.Entity.StateId stateId) {
			var state = await _stateManager.Read(date, stateId);
			if ( state == null ) {
				return Array.Empty<Currency.Entity.ReadOnlyCurrencyEntity>();
			}
			var currencyTasks = state.Currencies
				.Select(id => _currencyManager.Read(date, id));
			var currencies = await Task.WhenAll(currencyTasks);
			return currencies
				.Where(c => (c != null))
				.Select(c => c!)
				.ToArray();
		}
	}

	public sealed class CreateAccountUseCase {
		public async Task Handle(DateTimeOffset date, Shared.Entity.BrokerId broker, Shared.Entity.CurrencyId currency) {
			throw new NotImplementedException();
		}
	}

	public sealed class ReadAccountsUseCase {
		public async Task<IReadOnlyCollection<Account.Entity.ReadOnlyAccountEntity>> Handle(DateTimeOffset date, Shared.Entity.BrokerId broker) {
			throw new NotImplementedException();
		}
	}

	public sealed class AddIncomeUseCase {
		public async Task Handle(DateTimeOffset date, Shared.Entity.AccountId account, Shared.Entity.CurrencyId inCurrency, decimal amount, decimal exchangeRate) {
			throw new NotImplementedException();
		}
	}

	public sealed class AddExpenseUseCase {
		public async Task Handle(DateTimeOffset date, Shared.Entity.AccountId account, Shared.Entity.CurrencyId inCurrency, decimal amount, decimal exchangeRate) {
			throw new NotImplementedException();
		}
	}

	public sealed class ReadAccountUseCase {
		public async Task<Account.Entity.ReadOnlyAccountEntity?> Handle(DateTimeOffset date, Shared.Entity.AccountId account) {
			throw new NotImplementedException();
		}
	}
}