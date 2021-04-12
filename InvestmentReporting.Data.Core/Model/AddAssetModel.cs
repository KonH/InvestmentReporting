using System;

namespace InvestmentReporting.Data.Core.Model {
	public record AddAssetModel(
		DateTimeOffset Date, string User, string Broker, string Id, string Isin, int Count) : ICommandModel;
}