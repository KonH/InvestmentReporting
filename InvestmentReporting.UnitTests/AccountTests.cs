using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AccountTests {
		readonly DateTimeOffset _date       = DateTimeOffset.MinValue;
		readonly UserId         _userId     = new("user");
		readonly BrokerId       _brokerId   = new("broker");
		readonly CurrencyId     _currencyId = new("currency");

		[Test]
		public async Task IsAccountAdded() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateAccountUseCase(stateManager, new GuidIdGenerator());

			await createUseCase.Handle(_date, _userId, _brokerId, _currencyId, accountName);

			var state  = await stateManager.Read(_date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Accounts.Should().NotBeEmpty();
			broker.Accounts.Should().Contain(a => (a.Currency == _currencyId) && (a.DisplayName == accountName));
		}

		[Test]
		public void IsAccountWithEmptyNameFailedToAdd() {
			var stateManager  = GetStateManager();
			var createUseCase = new CreateAccountUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidAccountException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyId, string.Empty));
		}

		[Test]
		public async Task IsAccountWithSameNameFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateAccountUseCase(stateManager, new GuidIdGenerator());
			await createUseCase.Handle(_date, _userId, _brokerId, _currencyId, accountName);

			Assert.ThrowsAsync<DuplicateAccountException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyId, accountName));
		}

		[Test]
		public void IsAccountForUnknownBrokerFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateAccountUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<BrokerNotFoundException>(() => createUseCase.Handle(_date, _userId, new(string.Empty), _currencyId, accountName));
		}

		[Test]
		public void IsAccountWithUnknownCurrencyFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateAccountUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<CurrencyNotFoundException>(() => createUseCase.Handle(_date, _userId, _brokerId, new(string.Empty), accountName));
		}

		StateManager GetStateManager() => new StateManagerBuilder().With(_userId).With(_brokerId).With(_currencyId).Build();
	}
}