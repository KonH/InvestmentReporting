using System.Collections.Generic;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Market.Entity {
	public record VirtualState(
		IReadOnlyDictionary<CurrencyCode, CurrencyBalance> Summary,
		IReadOnlyCollection<VirtualBalance> Balances);
}