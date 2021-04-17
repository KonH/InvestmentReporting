using System.Collections.Generic;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardStateTag(
		string Tag, IReadOnlyCollection<DashboardAsset> Assets, IReadOnlyDictionary<CurrencyId, SumState> Sums);
}