namespace InvestmentReporting.Domain.Entity {
	public record AssetISIN(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetISIN isin) => isin.ToString();
	}
}