using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Market.Entity {
	public record VirtualAsset(
		AssetId Id, BrokerId Broker, AssetISIN Isin, string? Name, AssetType? Type, int Count,
		decimal RealPrice, decimal VirtualPrice, decimal RealSum, decimal VirtualSum, CurrencyId Currency);
}