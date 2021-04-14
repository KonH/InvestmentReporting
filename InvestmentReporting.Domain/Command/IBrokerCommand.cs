using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public interface IBrokerCommand : ICommand {
		BrokerId Broker { get; }
	}
}