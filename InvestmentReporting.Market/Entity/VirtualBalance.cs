using System.Collections.Generic;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Market.Entity {
	public record VirtualBalance(
		decimal RealSum, decimal VirtualSum, IReadOnlyCollection<VirtualAsset> Inventory, CurrencyId Currency);
}