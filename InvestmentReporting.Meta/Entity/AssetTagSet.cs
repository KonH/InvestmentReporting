using System.Collections.Generic;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Meta.Entity {
	public record AssetTagSet(AssetISIN Isin, string Name, ISet<AssetTag> Tags);
}