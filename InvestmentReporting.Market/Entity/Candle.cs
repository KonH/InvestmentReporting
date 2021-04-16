using System;

namespace InvestmentReporting.Market.Entity {
	public record Candle(DateTimeOffset Date, decimal Open, decimal Close, decimal Low, decimal High);
}