namespace InvestmentReporting.State.Entity {
	public sealed class Asset {
		public readonly AssetId      Id;
		public readonly AssetISIN    Isin;
		public readonly CurrencyCode Currency;

		public int Count;

		public Asset(AssetId id, AssetISIN isin, CurrencyCode currency, int count) {
			Id       = id;
			Isin     = isin;
			Currency = currency;
			Count    = count;
		}
	}
}