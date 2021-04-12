namespace InvestmentReporting.Market.Entity {
	public record AssetType(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetType isin) => isin.ToString();
	}
}