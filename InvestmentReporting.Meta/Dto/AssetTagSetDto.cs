namespace InvestmentReporting.Meta.Dto {
	public record AssetTagSetDto(
		string Isin, string Name, string[] Tags);
}