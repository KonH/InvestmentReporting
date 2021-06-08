using System;

namespace InvestmentReporting.Import.Dto {
	public record Exchange(
		DateTimeOffset Date, string FromCurrency, string ToCurrency, decimal Amount, decimal Sum, decimal Fee);
}