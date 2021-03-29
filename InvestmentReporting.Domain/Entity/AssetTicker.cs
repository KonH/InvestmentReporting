namespace InvestmentReporting.Domain.Entity {
	public record AssetTicker(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetTicker ticker) => ticker.ToString();
	}
}