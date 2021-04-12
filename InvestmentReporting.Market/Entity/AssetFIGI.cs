namespace InvestmentReporting.Market.Entity {
	public record AssetFIGI(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AssetFIGI isin) => isin.ToString();
	}
}