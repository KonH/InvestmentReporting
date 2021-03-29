using System.Collections.Generic;
using System.Linq;

namespace InvestmentReporting.Domain.Entity {
	public sealed class ReadOnlyBroker {
		public readonly BrokerId                             Id;
		public readonly string                               DisplayName;
		public readonly IReadOnlyCollection<ReadOnlyAccount> Accounts;
		public readonly IReadOnlyCollection<ReadOnlyAsset>   Inventory;

		public ReadOnlyBroker(Broker broker) {
			Id          = broker.Id;
			DisplayName = broker.DisplayName;
			Accounts    = broker.Accounts.Select(a => new ReadOnlyAccount(a)).ToArray();
			Inventory   = broker.Inventory.Select(a => new ReadOnlyAsset(a)).ToArray();
		}
	}
}