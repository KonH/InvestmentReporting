using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record AddAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AssetId Asset, AssetISIN Isin, int Count) : IAssetCommand;
}