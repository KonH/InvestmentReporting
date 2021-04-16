using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface ICurrencyPriceRepository {
		Task Add(CurrencyPriceModel metadata);
		Task Update(CurrencyPriceModel metadata);

		IReadOnlyCollection<CurrencyPriceModel> GetAll();
	}
}