using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record CreateAccountCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Id, CurrencyCode Currency, string DisplayName) : ICommand;
}