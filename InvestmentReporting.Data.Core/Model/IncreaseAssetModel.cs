using System;

namespace InvestmentReporting.Data.Core.Model {
	public record IncreaseAssetModel(
		DateTimeOffset Date, string User, string Broker, string Id, int Count) : ICommandModel;
}