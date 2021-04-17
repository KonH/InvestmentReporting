using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record IncreaseAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AssetId Id, int Count) : ICommand;
}