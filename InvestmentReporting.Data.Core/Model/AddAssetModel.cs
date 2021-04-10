using System;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Model {
	public record AddAssetModel(
		DateTimeOffset Date, string User, string Broker,
		string Id, string Name, string Category, string Isin, int Count) : ICommandModel {}
}