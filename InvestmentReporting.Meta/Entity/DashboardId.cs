namespace InvestmentReporting.Meta.Entity {
	public record DashboardId(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(DashboardId isin) => isin.ToString();
	}
}