using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record AddIncomeCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Account, OperationId Id,
		decimal Amount, IncomeCategory Category, AssetId? Asset) : IAccountCommand, IAssetCommand;
}