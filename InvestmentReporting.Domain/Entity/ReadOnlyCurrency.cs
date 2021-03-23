namespace InvestmentReporting.Domain.Entity {
	public sealed class ReadOnlyCurrency {
		public readonly CurrencyId     Id;
		public readonly CurrencyCode   Code;
		public readonly CurrencyFormat Format;

		public ReadOnlyCurrency(Currency currency) {
			Id     = currency.Id;
			Code   = currency.Code;
			Format = currency.Format;
		}
	}
}