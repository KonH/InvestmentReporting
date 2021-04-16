using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IAssetTagRepository {
		UserAssetTagsModel? Get(string user);

		Task Add(string user, string asset, string tag);

		Task Remove(string user, string asset, string tag);
	}
}