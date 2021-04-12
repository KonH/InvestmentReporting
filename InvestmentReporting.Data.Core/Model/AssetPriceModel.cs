using System.Collections.Generic;

namespace InvestmentReporting.Data.Core.Model {
	public record AssetPriceModel(string Isin, string Figi, List<AssetCandleModel> Candles);
}