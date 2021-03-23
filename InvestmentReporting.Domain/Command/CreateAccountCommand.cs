using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record CreateAccountCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Id, CurrencyId Currency, string DisplayName) : ICommand {}
}