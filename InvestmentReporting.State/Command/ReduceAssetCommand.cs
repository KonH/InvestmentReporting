using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record ReduceAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AssetId Asset, int Count) : IAssetCommand;
}