namespace InvestmentReporting.State.Entity {
	public record OperationId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(OperationId id) => id.ToString();
	}
}