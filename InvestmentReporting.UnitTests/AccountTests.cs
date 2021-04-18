using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AccountTests {
		readonly DateTimeOffset _date         = DateTimeOffset.MinValue;
		readonly UserId         _userId       = new("user");
		readonly BrokerId       _brokerId     = new("broker");
		readonly CurrencyCode   _currencyCode = new("USD");

		[Test]
		public async Task IsAccountAdded() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = GetUseCase(stateManager);

			await createUseCase.Handle(_date, _userId, _brokerId, _currencyCode, accountName);

			var state  = stateManager.ReadState(_date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Accounts.Should().NotBeEmpty();
			broker.Accounts.Should().Contain(a => (a.Currency == _currencyCode) && (a.DisplayName == accountName));
		}

		[Test]
		public void IsAccountWithEmptyNameFailedToAdd() {
			var stateManager  = GetStateManager();
			var createUseCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAccountException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyCode, string.Empty));
		}

		[Test]
		public async Task IsAccountWithSameNameFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = GetUseCase(stateManager);
			await createUseCase.Handle(_date, _userId, _brokerId, _currencyCode, accountName);

			Assert.ThrowsAsync<DuplicateAccountException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyCode, accountName));
		}

		[Test]
		public void IsAccountForUnknownBrokerFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<BrokerNotFoundException>(() => createUseCase.Handle(_date, _userId, new(string.Empty), _currencyCode, accountName));
		}

		[Test]
		public void IsAccountWithUnknownCurrencyFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager();
			var createUseCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<CurrencyNotFoundException>(() => createUseCase.Handle(_date, _userId, _brokerId, new(string.Empty), accountName));
		}

		StateManager GetStateManager() => new StateManagerBuilder().With(_userId).With(_brokerId).With(_currencyCode).Build();

		CreateAccountUseCase GetUseCase(StateManager stateManager) =>
			new CreateAccountUseCase(stateManager, new GuidIdGenerator(), new CurrencyConfiguration());
	}
}