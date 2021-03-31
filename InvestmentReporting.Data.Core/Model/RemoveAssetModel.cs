using System;

namespace InvestmentReporting.Data.Core.Model {
	public record RemoveAssetModel(
		DateTimeOffset Date, string User, string Broker, string Id) : ICommandModel {}
}