using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IAssetMetadataRepository {
		Task Add(AssetMetadataModel metadata);

		IReadOnlyCollection<AssetMetadataModel> GetAll();
	}
}