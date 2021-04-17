using System.Collections.Generic;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardState(IReadOnlyCollection<DashboardStateTag> Tags, IReadOnlyDictionary<CurrencyId, SumState> Sums);
}