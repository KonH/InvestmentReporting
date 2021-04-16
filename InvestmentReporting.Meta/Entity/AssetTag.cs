namespace InvestmentReporting.Meta.Entity {
	public record AssetTag(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetTag isin) => isin.ToString();
	}
}