using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IAssetPriceRepository {
		Task Add(AssetPriceModel metadata);
		Task Update(AssetPriceModel metadata);

		IReadOnlyCollection<AssetPriceModel> GetAll();

		Task Clear();
	}
}