using System;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class BrokerTests {
		readonly DateTimeOffset _date   = DateTimeOffset.MinValue;
		readonly UserId         _userId = new("user");

		[Test]
		public async Task IsBrokerAdded() {
			var brokerName    = "BrokerName";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateBrokerUseCase(stateManager, new GuidIdGenerator());

			await createUseCase.Handle(_date, _userId, brokerName);

			var state = stateManager.ReadState(_date, _userId);
			state.Brokers.Should().NotBeEmpty();
			state.Brokers.Should().Contain(b => b.DisplayName == brokerName);
		}

		[Test]
		public void IsBrokerWithEmptyNameFailedToAdd() {
			var stateManager  = GetStateManager();
			var createUseCase = new CreateBrokerUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidBrokerException>(() => createUseCase.Handle(_date, _userId, string.Empty));
		}

		[Test]
		public async Task IsBrokerWithDuplicateNameFailedToAdd() {
			var brokerName    = "BrokerName";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateBrokerUseCase(stateManager, new GuidIdGenerator());
			await createUseCase.Handle(_date, _userId, brokerName);

			Assert.ThrowsAsync<DuplicateBrokerException>(() => createUseCase.Handle(_date, _userId, brokerName));
		}

		[Test]
		public async Task IsBrokerNotAddedInPast() {
			var brokerName    = "BrokerName";
			var stateManager  = GetStateManager();
			var createUseCase = new CreateBrokerUseCase(stateManager, new GuidIdGenerator());

			await createUseCase.Handle(_date.AddSeconds(1), _userId, brokerName);

			var state = stateManager.ReadState(_date, _userId);
			state.Brokers.Should().BeEmpty();
		}

		StateManager GetStateManager() => new StateManagerBuilder().Build();
	}
}