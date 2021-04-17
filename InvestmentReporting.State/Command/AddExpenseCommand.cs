using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record AddExpenseCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Account, OperationId Id,
		decimal Amount, ExpenseCategory Category, AssetId? Asset) : IAccountCommand, IAssetCommand;
}