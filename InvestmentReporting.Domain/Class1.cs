using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.Model;
using InvestmentReporting.Domain.Repository;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain {
	namespace Entity {
		public record UserId(string Value) {
			public override string ToString() => Value;
		}

		public record BrokerId(string Value) {
			public override string ToString() => Value;
		}

		public record CurrencyId(string Value) {
			public override string ToString() => Value;
		}

		public record CurrencyCode(string Value) {
			public override string ToString() => Value;
		}

		public record CurrencyFormat(string Value) {
			public override string ToString() => Value;
		}

		public record AccountId(string Value) {
			public override string ToString() => Value;
		}

		public record OperationId(string Value) {
			public override string ToString() => Value;
		}

		public record IncomeCategory(string Value) {
			public override string ToString() => Value;
		}

		public record ExpenseCategory(string Value) {
			public override string ToString() => Value;
		}

		public record State(List<Broker> Brokers, List<Currency> Currencies) {}

		public record Broker(BrokerId Id, string DisplayName, List<Account> Accounts) {}

		public record Currency(CurrencyId Id, CurrencyCode Code, CurrencyFormat Format) {}

		public sealed class Account {
			public readonly AccountId  Id;
			public readonly CurrencyId Currency;
			public readonly string     DisplayName;

			public decimal Balance;

			public Account(AccountId id, CurrencyId currency, string displayName) {
				Id          = id;
				Currency    = currency;
				DisplayName = displayName;
			}
		}

		public sealed class ReadOnlyBroker {
			public readonly BrokerId                             Id;
			public readonly string                               DisplayName;
			public readonly IReadOnlyCollection<ReadOnlyAccount> Accounts;

			public ReadOnlyBroker(Broker broker) {
				Id          = broker.Id;
				DisplayName = broker.DisplayName;
				Accounts    = broker.Accounts.Select(a => new ReadOnlyAccount(a)).ToArray();
			}
		}

		public sealed class ReadOnlyCurrency {
			public readonly CurrencyId     Id;
			public readonly CurrencyCode   Code;
			public readonly CurrencyFormat Format;

			public ReadOnlyCurrency(Currency currency) {
				Id     = currency.Id;
				Code   = currency.Code;
				Format = currency.Format;
			}
		}

		public sealed class ReadOnlyAccount {
			public readonly AccountId  Id;
			public readonly CurrencyId Currency;
			public readonly string     DisplayName;
			public readonly decimal    Balance;

			public ReadOnlyAccount(Account account) {
				Id          = account.Id;
				Currency    = account.Currency;
				DisplayName = account.DisplayName;
				Balance     = account.Balance;
			}
		}

		public sealed class ReadOnlyState {
			public readonly IReadOnlyCollection<ReadOnlyBroker>   Brokers;
			public readonly IReadOnlyCollection<ReadOnlyCurrency> Currencies;

			public ReadOnlyState(State state) {
				Brokers    = state.Brokers.Select(b => new ReadOnlyBroker(b)).ToArray();
				Currencies = state.Currencies.Select(c => new ReadOnlyCurrency(c)).ToArray();
			}
		}
	}

	namespace Command {
		public interface ICommand {
			DateTimeOffset Date { get; }
			UserId         User { get; }
		}

		public record CreateBrokerCommand(
			DateTimeOffset Date, UserId User, BrokerId Id, string DisplayName) : ICommand {}

		public record CreateCurrencyCommand(
			DateTimeOffset Date, UserId User, CurrencyId Id, CurrencyCode Code, CurrencyFormat Format) : ICommand {}

		public record CreateAccountCommand(
			DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Id, CurrencyId Currency, string DisplayName) : ICommand {}

		public record AddIncomeCommand(
			DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Account, OperationId Id,
			CurrencyId Currency, decimal Amount, decimal ExchangeRate, IncomeCategory Category) : ICommand {}

		public record AddExpenseCommand(
			DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Account, OperationId Id,
			CurrencyId Currency, decimal Amount, decimal ExchangeRate, ExpenseCategory Category) : ICommand {}
	}

	namespace Model {
		public interface ICommandModel {
			DateTimeOffset Date  { get; }
			string         User { get; }
		}

		public record CreateBrokerModel(
			DateTimeOffset Date, string User, string Id, string DisplayName) : ICommandModel {}

		public record CreateCurrencyModel(
			DateTimeOffset Date, string User, string Id, string Code, string Format) : ICommandModel {}

		public record CreateAccountModel(
			DateTimeOffset Date, string User, string Broker, string Id, string Currency, string DisplayName) : ICommandModel {}

		public record AddIncomeModel(
			DateTimeOffset Date, string User, string Broker, string Account, string Id,
			string Currency, decimal Amount, decimal ExchangeRate, string Category) : ICommandModel {}

		public record AddExpenseModel(
			DateTimeOffset Date, string User, string Broker, string Account, string Id,
			string Currency, decimal Amount, decimal ExchangeRate, string Category) : ICommandModel {}
	}

	namespace Repository {
		public sealed class IdGenerator {
			public string GenerateNewId() => Guid.NewGuid().ToString();
		}

		public sealed class StateRepository {
			readonly List<ICommandModel> _commands;

			public StateRepository(List<ICommandModel> commands) {
				_commands = commands;
			}

			public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(DateTimeOffset date, UserId user) =>
				Task.FromResult((IReadOnlyCollection<ICommandModel>)_commands
					.Where(c => c.Date <= date)
					.Where(c => c.User == user.ToString())
					.ToArray());

			public Task SaveCommand(ICommandModel model) {
				_commands.Add(model);
				return Task.CompletedTask;
			}
		}
	}

	namespace Logic {
		public sealed class StateManager {
			readonly StateRepository _repository;

			public StateManager(StateRepository repository) {
				_repository = repository;
			}

			async Task<State> Take(DateTimeOffset date, UserId id) {
				var state         = new State(new List<Broker>(), new List<Currency>());
				var commandModels = await _repository.ReadCommands(date, id);
				foreach ( var model in commandModels ) {
					switch ( model ) {
						case CreateBrokerModel m: {
							state.Brokers.Add(new(
								new(m.Id), m.DisplayName, new List<Account>()));
							break;
						}
						case CreateCurrencyModel m: {
							state.Currencies.Add(new(
								new(m.Id), new(m.Code), new(m.Format)));
							break;
						}
						case CreateAccountModel m: {
							var brokerId = new BrokerId(m.Broker);
							var broker   = state.Brokers.First(b => b.Id == brokerId);
							broker.Accounts.Add(new(
								new(m.Id), new(m.Currency), m.DisplayName));
							break;
						}
						case AddIncomeModel m: {
							var brokerId  = new BrokerId(m.Broker);
							var broker    = state.Brokers.First(b => b.Id == brokerId);
							var accountId = new AccountId(m.Account);
							var account   = broker.Accounts.First(a => a.Id == accountId);
							account.Balance += m.Amount * m.ExchangeRate;
							break;
						}
						case AddExpenseModel m: {
							var brokerId  = new BrokerId(m.Broker);
							var broker    = state.Brokers.First(b => b.Id == brokerId);
							var accountId = new AccountId(m.Account);
							var account   = broker.Accounts.First(a => a.Id == accountId);
							account.Balance -= m.Amount * m.ExchangeRate;
							break;
						}
						default:
							throw new NotSupportedException(model.GetType().FullName);
					}
				}
				return state;
			}

			public async Task<ReadOnlyState> Read(DateTimeOffset date, UserId id) =>
				new(await Take(date, id));

			public async Task Push(ICommand command) {
				var date  = command.Date;
				var user = command.User.ToString();
				ICommandModel model = command switch {
					CreateBrokerCommand cmd => new CreateBrokerModel(
						date, user, cmd.Id.ToString(), cmd.DisplayName),
					CreateCurrencyCommand cmd => new CreateCurrencyModel(
						date, user, cmd.Id.ToString(), cmd.Code.ToString(), cmd.Format.ToString()),
					CreateAccountCommand cmd => new CreateAccountModel(
						date, user, cmd.Broker.ToString(), cmd.Id.ToString(), cmd.Currency.ToString(), cmd.DisplayName),
					AddIncomeCommand cmd => new AddIncomeModel(
						date, user, cmd.Broker.ToString(), cmd.Account.ToString(), cmd.Id.ToString(),
						cmd.Currency.ToString(), cmd.Amount, cmd.ExchangeRate, cmd.Category.ToString()),
					AddExpenseCommand cmd => new AddExpenseModel(
						date, user, cmd.Broker.ToString(), cmd.Account.ToString(), cmd.Id.ToString(),
						cmd.Currency.ToString(), cmd.Amount, cmd.ExchangeRate, cmd.Category.ToString()),
					_  => throw new NotSupportedException(command.GetType().FullName)
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

			public async Task<ReadOnlyState> Handle(DateTimeOffset date, UserId user) {
				if ( string.IsNullOrWhiteSpace(user.Value) ) {
					throw new InvalidUserException();
				}
				return await _stateManager.Read(date, user);
			}
		}

		public sealed class CreateBrokerUseCase {
			readonly StateManager _stateManager;
			readonly IdGenerator  _idGenerator;

			public CreateBrokerUseCase(StateManager stateManager, IdGenerator idGenerator) {
				_stateManager = stateManager;
				_idGenerator  = idGenerator;
			}

			public async Task Handle(DateTimeOffset date, UserId user, string displayName) {
				if ( string.IsNullOrWhiteSpace(displayName) ) {
					throw new InvalidBrokerException();
				}
				var state = await _stateManager.Read(date, user);
				if ( state.Brokers.Any(b => b.DisplayName == displayName) ) {
					throw new DuplicateBrokerException();
				}
				var id = new BrokerId(_idGenerator.GenerateNewId());
				await _stateManager.Push(new CreateBrokerCommand(date, user, id, displayName));
			}
		}

		public sealed class CreateCurrencyUseCase {
			readonly StateManager _stateManager;
			readonly IdGenerator  _idGenerator;

			public CreateCurrencyUseCase(StateManager stateManager, IdGenerator idGenerator) {
				_stateManager = stateManager;
				_idGenerator  = idGenerator;
			}

			public async Task Handle(DateTimeOffset date, UserId user, CurrencyCode code, CurrencyFormat format) {
				if ( string.IsNullOrWhiteSpace(code.ToString()) ) {
					throw new InvalidCurrencyException();
				}
				if ( !format.ToString().Contains("{0}") ) {
					throw new InvalidCurrencyException();
				}
				var id = new CurrencyId(_idGenerator.GenerateNewId());
				await _stateManager.Push(new CreateCurrencyCommand(date, user, id, code, format));
			}
		}

		public sealed class CreateAccountUseCase {
			readonly StateManager _stateManager;
			readonly IdGenerator  _idGenerator;

			public CreateAccountUseCase(StateManager stateManager, IdGenerator idGenerator) {
				_stateManager = stateManager;
				_idGenerator  = idGenerator;
			}

			public async Task Handle(DateTimeOffset date, UserId user, BrokerId broker, CurrencyId currency, string displayName) {
				if ( string.IsNullOrWhiteSpace(displayName) ) {
					throw new InvalidAccountException();
				}
				var state = await _stateManager.Read(date, user);
				if ( state.Brokers.All(b => b.Id != broker) ) {
					throw new BrokerNotFoundException();
				}
				if ( state.Currencies.All(c => c.Id != currency) ) {
					throw new CurrencyNotFoundException();
				}
				var id = new AccountId(_idGenerator.GenerateNewId());
				await _stateManager.Push(new CreateAccountCommand(date, user, broker, id, currency, displayName));
			}
		}

		public sealed class AddIncomeUseCase {
			readonly StateManager _stateManager;
			readonly IdGenerator  _idGenerator;

			public AddIncomeUseCase(StateManager stateManager, IdGenerator idGenerator) {
				_stateManager = stateManager;
				_idGenerator  = idGenerator;
			}

			public async Task Handle(
				DateTimeOffset date, UserId user, BrokerId broker, AccountId account,
				CurrencyId currency, decimal amount, decimal exchangeRate, IncomeCategory category) {
				if ( amount == 0 ) {
					throw new InvalidPriceException();
				}
				if ( exchangeRate == 0 ) {
					throw new InvalidPriceException();
				}
				if ( string.IsNullOrWhiteSpace(category.ToString()) ) {
					throw new InvalidCategoryException();
				}
				var state = await _stateManager.Read(date, user);
				if ( state.Currencies.All(c => c.Id != currency) ) {
					throw new CurrencyNotFoundException();
				}
				var brokerState = state.Brokers.FirstOrDefault(b => b.Id == broker);
				if ( brokerState == null ) {
					throw new BrokerNotFoundException();
				}
				if ( brokerState.Accounts.All(a => a.Id != account) ) {
					throw new AccountNotFoundException();
				}
				var id = new OperationId(_idGenerator.GenerateNewId());
				await _stateManager.Push(new AddIncomeCommand(
					date, user, broker, account, id, currency, amount, exchangeRate, category));
			}
		}

		public sealed class AddExpenseUseCase {
			readonly StateManager _stateManager;
			readonly IdGenerator  _idGenerator;

			public AddExpenseUseCase(StateManager stateManager, IdGenerator idGenerator) {
				_stateManager = stateManager;
				_idGenerator  = idGenerator;
			}

			public async Task Handle(
				DateTimeOffset date, UserId user, BrokerId broker, AccountId account,
				CurrencyId currency, decimal amount, decimal exchangeRate, ExpenseCategory category) {
				if ( amount == 0 ) {
					throw new InvalidPriceException();
				}
				if ( exchangeRate == 0 ) {
					throw new InvalidPriceException();
				}
				if ( string.IsNullOrWhiteSpace(category.ToString()) ) {
					throw new InvalidCategoryException();
				}
				var state = await _stateManager.Read(date, user);
				if ( state.Currencies.All(c => c.Id != currency) ) {
					throw new CurrencyNotFoundException();
				}
				var brokerState = state.Brokers.FirstOrDefault(b => b.Id == broker);
				if ( brokerState == null ) {
					throw new BrokerNotFoundException();
				}
				if ( brokerState.Accounts.All(a => a.Id != account) ) {
					throw new AccountNotFoundException();
				}
				var id = new OperationId(_idGenerator.GenerateNewId());
				await _stateManager.Push(new AddExpenseCommand(
					date, user, broker, account, id, currency, amount, exchangeRate, category));
			}
		}
	}

	namespace UseCase.Exceptions {
		public sealed class InvalidUserException : Exception {}

		public sealed class InvalidBrokerException : Exception {}

		public sealed class DuplicateBrokerException : Exception {}

		public sealed class InvalidCurrencyException : Exception {}

		public sealed class BrokerNotFoundException : Exception {}

		public sealed class InvalidAccountException : Exception {}

		public sealed class AccountNotFoundException : Exception {}

		public sealed class CurrencyNotFoundException : Exception {}

		public sealed class InvalidCategoryException : Exception {}

		public sealed class InvalidPriceException : Exception {}
	}
}