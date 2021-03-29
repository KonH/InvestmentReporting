namespace InvestmentReporting.Domain.Entity {
	public record AssetCategory(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetCategory category) => category.ToString();
	}
}