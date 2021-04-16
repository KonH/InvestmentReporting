using System.Collections.Generic;

namespace InvestmentReporting.Meta.Entity {
	public record AssetTagState(IReadOnlyCollection<AssetTagSet> Assets);
}