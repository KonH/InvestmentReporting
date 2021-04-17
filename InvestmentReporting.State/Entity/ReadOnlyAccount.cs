namespace InvestmentReporting.State.Entity {
	public sealed class ReadOnlyAccount {
		public readonly AccountId  Id;
		public readonly CurrencyId Currency;
		public readonly string     DisplayName;
		public readonly decimal    Balance;

		public ReadOnlyAccount(Account account) {
			Id          = account.Id;
			Currency    = account.Currency;
			DisplayName = account.DisplayName;
			Balance     = account.Balance;
		}
	}
}