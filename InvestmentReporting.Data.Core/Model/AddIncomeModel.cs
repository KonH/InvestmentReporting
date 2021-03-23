using System;

namespace InvestmentReporting.Data.Core.Model {
	public record AddIncomeModel(
		DateTimeOffset Date, string User, string Broker, string Account, string Id,
		string Currency, decimal Amount, decimal ExchangeRate, string Category) : ICommandModel {}
}