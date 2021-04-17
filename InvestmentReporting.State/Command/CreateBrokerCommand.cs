using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public record CreateBrokerCommand(
		DateTimeOffset Date, UserId User, BrokerId Id, string DisplayName) : ICommand;
}