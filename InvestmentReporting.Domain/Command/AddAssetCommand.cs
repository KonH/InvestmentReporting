using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record AddAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker,
		AssetId Id, string Name, AssetCategory Category, AssetTicker Ticker, int Count) : ICommand {}
}