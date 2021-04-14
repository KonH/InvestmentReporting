using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public record CreateBrokerCommand(
		DateTimeOffset Date, UserId User, BrokerId Id, string DisplayName) : ICommand;
}