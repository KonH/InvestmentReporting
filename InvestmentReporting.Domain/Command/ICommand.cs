using System;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public interface ICommand {
		DateTimeOffset Date { get; }
		UserId         User { get; }
	}
}