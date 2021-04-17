namespace InvestmentReporting.State.Entity {
	public record CurrencyId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(CurrencyId id) => id.ToString();
	}
}