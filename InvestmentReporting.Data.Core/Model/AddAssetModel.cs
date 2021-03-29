using System;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Domain.Command {
	public record AddAssetModel(
		DateTimeOffset Date, string User, string Broker,
		string Id, string Name, string Category, string Ticker,
		string BoughtCurrency, decimal BoughtPrice, string FeeCurrency, decimal BoughtFee, int Count) : ICommandModel {}
}