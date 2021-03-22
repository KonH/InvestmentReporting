using System;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public class Tests {
		[Test]
		public async Task IsStateCreated() {
			var date         = DateTimeOffset.MinValue;
			var userId       = new UserId(string.Empty);
			var stateManager = new StateManager();
			var readUseCase  = new ReadStateUseCase(stateManager);

			var state = await readUseCase.Handle(date, userId);

			state.Should().NotBeNull();
		}

		/*[Test]
		public async Task IsBrokerAdded() {
			var userId        = new UserId(string.Empty);
			var date          = DateTimeOffset.MinValue;
			var stateManager  = new StateManager();
			var brokerManager = new BrokerManager();
			var createUseCase = new OpenBrokerUseCase(stateManager, brokerManager, new IdGenerator());
			var readUseCase   = new ReadBrokersUseCase(stateManager, brokerManager);
			var stateId       = await new EnsureStateUseCase(stateManager, new IdGenerator()).Handle(date, userId);

			await createUseCase.Handle(date, userId, stateId, "BrokerName");

			var brokers = await readUseCase.Handle(date, stateId);
			brokers.Should().NotBeEmpty();
		}

		[Test]
		public async Task IsBrokerNotAddedInPast() {
			var userId        = new UserId(string.Empty);
			var stateManager  = new StateManager();
			var brokerManager = new BrokerManager();
			var createUseCase = new OpenBrokerUseCase(stateManager, brokerManager, new IdGenerator());
			var readUseCase   = new ReadBrokersUseCase(stateManager, brokerManager);
			var stateId       = await new EnsureStateUseCase(stateManager, new IdGenerator()).Handle(DateTimeOffset.MinValue, userId);

			await createUseCase.Handle(DateTimeOffset.MinValue.AddSeconds(1), userId, stateId, "BrokerName");

			var brokers = await readUseCase.Handle(DateTimeOffset.MinValue, stateId);
			brokers.Should().BeEmpty();
		}

		[Test]
		public async Task IsCurrencyAdded() {
			var user            = new UserId(string.Empty);
			var stateManager    = new StateManager();
			var currencyManager = new CurrencyManager();
			var createUseCase   = new CreateCurrencyUseCase(stateManager, currencyManager, new IdGenerator());
			var readUseCase     = new ReadCurrenciesUseCase(stateManager, currencyManager);
			var stateId         = await new EnsureStateUseCase(stateManager, new IdGenerator()).Handle(DateTimeOffset.MinValue, user);

			await createUseCase.Handle(DateTimeOffset.MinValue, user, stateId, code: "USD", format: "${0}");

			var currencies = await readUseCase.Handle(DateTimeOffset.MinValue, stateId);
			currencies.Should().NotBeEmpty();
		}*/

		/*[Test]
		public async Task IsAccountAdded() {
			var broker        = new BrokerId(string.Empty);
			var currency      = new CurrencyId(string.Empty);
			var createUseCase = new CreateAccountUseCase();
			var readUseCase   = new ReadAccountsUseCase();

			await createUseCase.Handle(DateTimeOffset.MinValue, broker, currency);

			var accounts = await readUseCase.Handle(DateTimeOffset.MinValue, broker);
			accounts.Should().NotBeEmpty();
		}

		[Test]
		public async Task IsIncomeAdded() {
			var account     = new AccountId(string.Empty);
			var currency    = new CurrencyId(string.Empty);
			var addUseCase  = new AddIncomeUseCase();
			var readUseCase = new ReadAccountUseCase();

			await addUseCase.Handle(DateTimeOffset.MinValue, account, currency, 100, exchangeRate: 1);

			var state = await readUseCase.Handle(DateTimeOffset.MinValue, account);
			state.Should().NotBeNull();
			state!.Sum.Should().Be(100);
		}

		[Test]
		public async Task IsExpenseAdded() {
			var account     = new AccountId(string.Empty);
			var currency    = new CurrencyId(string.Empty);
			var addUseCase  = new AddExpenseUseCase();
			var readUseCase = new ReadAccountUseCase();

			await addUseCase.Handle(DateTimeOffset.MinValue, account, currency, 100, exchangeRate: 1);

			var state = await readUseCase.Handle(DateTimeOffset.MinValue, account);
			state.Should().NotBeNull();
			state!.Sum.Should().Be(100);
		}*/
	}
}