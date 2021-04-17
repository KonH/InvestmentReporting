using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public interface IBrokerCommand : ICommand {
		BrokerId Broker { get; }
	}
}