using System;

namespace InvestmentReporting.Data.Core.Model {
	public record CandleModel(DateTimeOffset Date, decimal Open, decimal Close, decimal Low, decimal High);
}