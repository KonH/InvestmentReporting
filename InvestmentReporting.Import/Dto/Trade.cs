using System;

namespace InvestmentReporting.Import.Dto {
	public record Trade(DateTimeOffset Date, string Isin, string Name, int Count, string Currency, decimal Sum, decimal Fee);
}