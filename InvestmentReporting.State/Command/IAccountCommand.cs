using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public interface IAccountCommand : IBrokerCommand {
		AccountId Account { get; }
	}
}