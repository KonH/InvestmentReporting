using System.Collections.Generic;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardAsset(AssetISIN Isin, string Name, IReadOnlyDictionary<CurrencyCode, SumState> Sums);
}