namespace InvestmentReporting.Domain.Entity {
	public sealed class Account {
		public readonly AccountId  Id;
		public readonly CurrencyId Currency;
		public readonly string     DisplayName;

		public decimal Balance;

		public Account(AccountId id, CurrencyId currency, string displayName) {
			Id          = id;
			Currency    = currency;
			DisplayName = displayName;
		}
	}
}