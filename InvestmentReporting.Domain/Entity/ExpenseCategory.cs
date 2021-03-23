namespace InvestmentReporting.Domain.Entity {
	public record ExpenseCategory(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(ExpenseCategory id) => id.ToString();
	}
}