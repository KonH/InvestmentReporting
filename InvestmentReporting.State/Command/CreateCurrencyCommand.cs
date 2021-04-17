using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record CreateCurrencyCommand(
		DateTimeOffset Date, UserId User, CurrencyId Id, CurrencyCode Code, CurrencyFormat Format) : ICommand;
}