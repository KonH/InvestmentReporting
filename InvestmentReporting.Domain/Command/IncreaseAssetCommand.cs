using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record IncreaseAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AssetId Id, int Count) : ICommand {}
}