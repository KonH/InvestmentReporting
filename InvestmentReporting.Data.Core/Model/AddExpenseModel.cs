using System;

namespace InvestmentReporting.Data.Core.Model {
	public record AddExpenseModel(
		DateTimeOffset Date, string User, string Broker, string Account, string Id,
		decimal Amount, string Category, string? Asset) : ICommandModel {}
}