using System;
using FluentAssertions;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class StateTests {
		[Test]
		public void IsStateCreated() {
			var stateManager = new StateManagerBuilder().Build();
			var readUseCase  = new ReadStateUseCase(stateManager);

			var state = readUseCase.Handle(DateTimeOffset.MinValue, new("user"));

			state.Should().NotBeNull();
		}

		[Test]
		public void IsStateFailedToCreateForUnknownUser() {
			var user         = new UserId(string.Empty);
			var stateManager = new StateManagerBuilder().Build();
			var readUseCase  = new ReadStateUseCase(stateManager);

			Assert.Throws<InvalidUserException>(() => readUseCase.Handle(DateTimeOffset.MinValue, user));
		}
	}
}