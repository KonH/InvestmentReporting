using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public interface ICommand {
		DateTimeOffset Date { get; }
		UserId         User { get; }
	}
}