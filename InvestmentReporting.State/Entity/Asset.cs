namespace InvestmentReporting.State.Entity {
	public sealed class Asset {
		public readonly AssetId      Id;
		public readonly AssetISIN    Isin;
		public readonly CurrencyCode Currency;
		public readonly string       RawName;

		public int Count;

		public Asset(AssetId id, AssetISIN isin, CurrencyCode currency, string rawName, int count) {
			Id       = id;
			Isin     = isin;
			Currency = currency;
			Count    = count;
			RawName  = rawName;
		}
	}
}