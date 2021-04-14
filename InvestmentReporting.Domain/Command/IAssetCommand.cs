using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Command {
	public interface IAssetCommand : IBrokerCommand {
		AssetId? Asset { get; }
	}
}