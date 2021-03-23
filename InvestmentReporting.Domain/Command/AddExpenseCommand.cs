using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record AddExpenseCommand(
		DateTimeOffset Date, UserId User, BrokerId Broker, AccountId Account, OperationId Id,
		CurrencyId Currency, decimal Amount, decimal ExchangeRate, ExpenseCategory Category) : ICommand {}
}