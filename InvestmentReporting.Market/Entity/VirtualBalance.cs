using System.Collections.Generic;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Market.Entity {
	public record VirtualBalance(
		decimal RealSum, decimal VirtualSum, IReadOnlyCollection<VirtualAsset> Inventory, CurrencyCode Currency);
}