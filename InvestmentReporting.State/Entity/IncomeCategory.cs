namespace InvestmentReporting.State.Entity {
	public record IncomeCategory(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(IncomeCategory id) => id.ToString();

		public static readonly IncomeCategory SellAsset = new("Asset Sell");
		public static readonly IncomeCategory Transfer  = new("Income Transfer");
		public static readonly IncomeCategory Dividend  = new("Share Dividend");
		public static readonly IncomeCategory Coupon    = new("Bond Coupon");
	}
}