using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Market.Entity {
	public record AssetMetadata(AssetISIN Isin, AssetFIGI Figi, string Name, AssetType Type);
}