using System.Collections.Generic;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardStateTag(
		string Tag, IReadOnlyCollection<DashboardAsset> Assets, IReadOnlyDictionary<CurrencyCode, SumState> Sums);
}