using System.Collections.Generic;

namespace InvestmentReporting.Meta.Entity {
	public record AssetTagState(ISet<AssetTag> Tags, IReadOnlyCollection<AssetTagSet> Assets);
}