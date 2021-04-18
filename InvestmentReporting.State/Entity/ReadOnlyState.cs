using System.Collections.Generic;
using System.Linq;

namespace InvestmentReporting.State.Entity {
	public sealed class ReadOnlyState {
		public readonly IReadOnlyCollection<ReadOnlyBroker> Brokers;

		public ReadOnlyState(State state) {
			Brokers = state.Brokers.Select(b => new ReadOnlyBroker(b)).ToArray();
		}
	}
}