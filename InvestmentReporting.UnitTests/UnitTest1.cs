using System;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Domain;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public class Tests {
		[Test]
		public async Task IsBrokerAdded() {
			var userId        = new UserId(string.Empty);
			var date          = DateTimeOffset.MinValue;
			var stateManager  = new StateManager();
			var brokerManager = new BrokerManager();
			var createUseCase = new OpenBrokerUseCase(stateManager, brokerManager, new IdGenerator());
			var readUseCase   = new ReadBrokersUseCase(stateManager, brokerManager);

			await createUseCase.Handle(date, userId, "BrokerName");

			var brokers = await readUseCase.Handle(date, userId);
			brokers.Should().NotBeEmpty();
		}

		[Test]
		public async Task IsBrokerNotAddedInPast() {
			var userId        = new UserId(string.Empty);
			var stateManager  = new StateManager();
			var brokerManager = new BrokerManager();
			var createUseCase = new OpenBrokerUseCase(stateManager, brokerManager, new IdGenerator());
			var readUseCase   = new ReadBrokersUseCase(stateManager, brokerManager);

			await createUseCase.Handle(DateTimeOffset.MinValue.AddSeconds(1), userId, "BrokerName");

			var brokers = await readUseCase.Handle(DateTimeOffset.MinValue, userId);
			brokers.Should().BeEmpty();
		}
	}
}