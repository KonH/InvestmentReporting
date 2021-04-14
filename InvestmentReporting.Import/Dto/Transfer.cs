using System;

namespace InvestmentReporting.Import.Dto {
	public record Transfer(DateTimeOffset Date, string Comment, string Currency, decimal Amount);
}