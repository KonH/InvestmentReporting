using System;

namespace InvestmentReporting.Market.Entity {
	public record AssetCandle(DateTimeOffset Date, decimal Open, decimal Close, decimal Low, decimal High);
}