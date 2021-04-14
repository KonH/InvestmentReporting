using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record ReduceAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AssetId Asset, int Count) : IAssetCommand;
}