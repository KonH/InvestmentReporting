using System.Collections.Generic;
using System.Linq;

namespace InvestmentReporting.State.Entity {
	public sealed class ReadOnlyState {
		public readonly IReadOnlyCollection<ReadOnlyBroker>   Brokers;
		public readonly IReadOnlyCollection<ReadOnlyCurrency> Currencies;

		public ReadOnlyState(State state) {
			Brokers    = state.Brokers.Select(b => new ReadOnlyBroker(b)).ToArray();
			Currencies = state.Currencies.Select(c => new ReadOnlyCurrency(c)).ToArray();
		}
	}
}