using System;

namespace InvestmentReporting.Data.Core.Model {
	public record CreateCurrencyModel(
		DateTimeOffset Date, string User, string Id, string Code, string Format) : ICommandModel {}
}