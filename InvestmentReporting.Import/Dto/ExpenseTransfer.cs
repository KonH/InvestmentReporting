using System;

namespace InvestmentReporting.Import.Dto {
	public record ExpenseTransfer(DateTimeOffset Date, string Comment, string Currency, decimal Amount) {}
}