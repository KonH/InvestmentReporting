namespace InvestmentReporting.Domain.Entity {
	public record CurrencyCode(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(CurrencyCode id) => id.ToString();
	}
}