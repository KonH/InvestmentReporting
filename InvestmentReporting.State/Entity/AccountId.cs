namespace InvestmentReporting.State.Entity {
	public record AccountId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(AccountId id) => id.ToString();
	}
}