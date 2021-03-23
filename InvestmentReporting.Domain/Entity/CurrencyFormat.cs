namespace InvestmentReporting.Domain.Entity {
	public record CurrencyFormat(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(CurrencyFormat id) => id.ToString();
	}
}