using System.Collections.Generic;

namespace InvestmentReporting.Market.Entity {
	public record VirtualState(IReadOnlyCollection<VirtualBalance> Balances, IReadOnlyCollection<VirtualAsset> Inventory);
}