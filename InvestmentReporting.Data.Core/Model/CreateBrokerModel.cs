using System;

namespace InvestmentReporting.Data.Core.Model {
	public record CreateBrokerModel(
		DateTimeOffset Date, string User, string Id, string DisplayName) : ICommandModel;
}