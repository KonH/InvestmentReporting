using System;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class CurrencyTests {
		readonly DateTimeOffset _date   = DateTimeOffset.MinValue;
		readonly UserId         _userId = new("user");

		[Test]
		public async Task IsCurrencyAdded() {
			var code          = new CurrencyCode("USD");
			var format        = new CurrencyFormat("${0}");
			var stateManager  = GetStateManager();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new GuidIdGenerator());

			await createUseCase.Handle(_date, _userId, code, format);

			var state = await stateManager.ReadState(_date, _userId);
			state.Currencies.Should().NotBeEmpty();
			state.Currencies.Should().Contain(c => (c.Code == code) && (c.Format == format));
		}

		[Test]
		public async Task IsCurrencyWithSameCodeFailedToAdd() {
			var code          = new CurrencyCode("USD");
			var format        = new CurrencyFormat("${0}");
			var stateManager  = GetStateManager();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new GuidIdGenerator());
			await createUseCase.Handle(_date, _userId, code, format);

			Assert.ThrowsAsync<DuplicateCurrencyException>(() => createUseCase.Handle(_date, _userId, code, format));
		}

		[Test]
		public void IsCurrencyWithEmptyCodeFailedToAdd() {
			var code          = new CurrencyCode(string.Empty);
			var format        = new CurrencyFormat("${0}");
			var stateManager  = GetStateManager();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidCurrencyException>(() => createUseCase.Handle(_date, _userId, code, format));
		}

		[Test]
		public void IsCurrencyWithInvalidFormatFailedToAdd() {
			var code          = new CurrencyCode("USD");
			var format        = new CurrencyFormat("$");
			var stateManager  = GetStateManager();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidCurrencyException>(() => createUseCase.Handle(_date, _userId, code, format));
		}

		StateManager GetStateManager() => new StateManagerBuilder().Build();
	}
}