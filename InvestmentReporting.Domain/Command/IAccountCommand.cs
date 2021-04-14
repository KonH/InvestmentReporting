using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public interface IAccountCommand : IBrokerCommand {
		AccountId Account { get; }
	}
}