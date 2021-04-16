using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IAssetTagRepository {
		Task<UserAssetTagsModel?> Get(string user);

		Task Add(UserAssetTagsModel model);

		Task Update(UserAssetTagsModel model);
	}
}