namespace InvestmentReporting.State.Entity {
	public record UserId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(UserId id) => id.ToString();
	}
}