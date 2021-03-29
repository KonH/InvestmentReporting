using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record AddAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker,
		AssetId Id, string Name, AssetCategory Category, AssetTicker Ticker,
		CurrencyId BoughtCurrency, decimal BoughtPrice, int Count) : ICommand {}
}