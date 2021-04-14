namespace InvestmentReporting.Domain.Entity {
	public record ExpenseCategory(string Value) {
		public override string ToString() => Value;

		public static implicit operator string(ExpenseCategory id) => id.ToString();

		public static readonly ExpenseCategory BuyAsset     = new("Asset Buy");
		public static readonly ExpenseCategory BuyAssetFee  = new("Asset Buy Broker Fee");
		public static readonly ExpenseCategory SellAssetFee = new("Asset Sell Broker Fee");
		public static readonly ExpenseCategory Transfer     = new("Expense Transfer");
	}
}