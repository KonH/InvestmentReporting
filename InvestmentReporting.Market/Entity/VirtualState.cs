using System.Collections.Generic;

namespace InvestmentReporting.Market.Entity {
	public record VirtualState(IReadOnlyCollection<VirtualAsset> Inventory);
}