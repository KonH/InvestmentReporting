namespace InvestmentReporting.Domain.Entity {
	public record AssetId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetId id) => id.ToString();
	}
}