using System;

namespace InvestmentReporting.Data.Core.Model {
	public record CreateAccountModel(
		DateTimeOffset Date, string User, string Broker, string Id, string Currency, string DisplayName) : ICommandModel;
}