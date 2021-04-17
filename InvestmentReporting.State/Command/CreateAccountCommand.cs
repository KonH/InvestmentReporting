using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record CreateAccountCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Id, CurrencyId Currency, string DisplayName) : ICommand;
}