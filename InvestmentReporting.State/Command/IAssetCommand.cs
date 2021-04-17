using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Command {
	public interface IAssetCommand : IBrokerCommand {
		AssetId? Asset { get; }
	}
}