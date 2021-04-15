namespace InvestmentReporting.Market.Dto {
	public record VirtualAssetDto(
		string Id, string Broker, string Isin, string Name, string Type, int Count,
		decimal RealPrice, decimal VirtualPrice, decimal RealSum, decimal VirtualSum, string Currency);
}