using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Domain;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public class Tests {
		[Test]
		public async Task IsBrokerAdded() {
			var stateManager = new StateManager();
			await stateManager.Push(DateTimeOffset.MinValue, new UserId(string.Empty), new OpenBroker(new BrokerId(string.Empty)));
			var updatedState = await stateManager.Read(DateTimeOffset.MinValue, new UserId(string.Empty));

			updatedState.Brokers.Should().NotBeEmpty();
		}

		[Test]
		public async Task IsBrokerNotAddedInPastState() {
			var stateManager = new StateManager();
			await stateManager.Push(DateTimeOffset.MinValue.AddSeconds(1), new UserId(string.Empty), new OpenBroker(new BrokerId(string.Empty)));
			var updatedState = await stateManager.Read(DateTimeOffset.MinValue, new UserId(string.Empty));

			updatedState.Brokers.Should().BeEmpty();
		}
	}
}