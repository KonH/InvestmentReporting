using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record RemoveAssetCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AssetId Id) : ICommand {}
}