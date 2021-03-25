using System;

namespace InvestmentReporting.Import.Dto {
	public record IncomeTransfer(DateTimeOffset Date, string Comment, string Currency, decimal Amount) {}
}