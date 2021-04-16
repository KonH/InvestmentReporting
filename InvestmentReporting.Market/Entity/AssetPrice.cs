using System.Collections.Generic;

namespace InvestmentReporting.Market.Entity {
	public record AssetPrice(string Isin, string Figi, IReadOnlyList<Candle> Candles);
}