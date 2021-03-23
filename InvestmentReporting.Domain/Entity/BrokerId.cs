namespace InvestmentReporting.Domain.Entity {
	public record BrokerId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(BrokerId id) => id.ToString();
	}
}