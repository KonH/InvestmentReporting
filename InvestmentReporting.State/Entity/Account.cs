namespace InvestmentReporting.State.Entity {
	public sealed class Account {
		public readonly AccountId    Id;
		public readonly CurrencyCode Currency;
		public readonly string       DisplayName;

		public decimal Balance;

		public Account(AccountId id, CurrencyCode currency, string displayName) {
			Id          = id;
			Currency    = currency;
			DisplayName = displayName;
		}
	}
}