using System.Collections.Generic;

namespace InvestmentReporting.Data.Core.Model {
	public record CurrencyPriceModel(string Code, string Figi, List<CandleModel> Candles);
}