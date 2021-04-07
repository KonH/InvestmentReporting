using System;

namespace InvestmentReporting.Import.Dto {
	public record Trade(
		DateTimeOffset Date, string Isin, string Name, string Category, int Count, string Currency, decimal Sum, decimal Fee);
}