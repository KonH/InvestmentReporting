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
	public sealed class StateTests {
		[Test]
		public async Task IsStateCreated() {
			var stateManager = new StateManager(new InMemoryStateRepository(new()));
			var readUseCase  = new ReadStateUseCase(stateManager);

			var state = await readUseCase.Handle(DateTimeOffset.MinValue, new("user"));

			state.Should().NotBeNull();
		}

		[Test]
		public void IsStateFailedToCreateForUnknownUser() {
			var user         = new UserId(string.Empty);
			var stateManager = new StateManager(new InMemoryStateRepository(new()));
			var readUseCase  = new ReadStateUseCase(stateManager);

			Assert.ThrowsAsync<InvalidUserException>(() => readUseCase.Handle(DateTimeOffset.MinValue, user));
		}
	}
}