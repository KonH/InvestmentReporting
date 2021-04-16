using System.Collections.Generic;

namespace InvestmentReporting.Market.Entity {
	public record CurrencyPrice(string Code, string Figi, IReadOnlyList<Candle> Candles);
}