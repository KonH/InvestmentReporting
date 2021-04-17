using System.Collections.Generic;

namespace InvestmentReporting.State.Entity {
	public record Broker(BrokerId Id, string DisplayName, List<Account> Accounts, List<Asset> Inventory);
}