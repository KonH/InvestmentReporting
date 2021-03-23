namespace InvestmentReporting.Domain.Entity {
	public record IncomeCategory(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(IncomeCategory id) => id.ToString();
	}
}