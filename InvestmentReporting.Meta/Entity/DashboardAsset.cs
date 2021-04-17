using System.Collections.Generic;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardAsset(AssetISIN Isin, string Name, IReadOnlyDictionary<CurrencyId, SumState> Sums);
}