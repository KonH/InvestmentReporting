using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record CreateCurrencyCommand(
		DateTimeOffset Date, UserId User, CurrencyId Id, CurrencyCode Code, CurrencyFormat Format) : ICommand;
}