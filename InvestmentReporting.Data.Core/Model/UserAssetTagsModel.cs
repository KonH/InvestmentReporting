using System.Collections.Generic;

namespace InvestmentReporting.Data.Core.Model {
	public record UserAssetTagsModel(string User, Dictionary<string, List<string>> Tags);
}