namespace InvestmentReporting.Market.Dto {
	public record VirtualBalanceDto(decimal RealSum, decimal VirtualSum, VirtualAssetDto[] Inventory, string Currency);
}