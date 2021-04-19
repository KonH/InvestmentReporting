namespace InvestmentReporting.State.Entity {
	public sealed class ReadOnlyAsset {
		public readonly AssetId      Id;
		public readonly AssetISIN    Isin;
		public readonly CurrencyCode Currency;
		public readonly string       RawName;
		public readonly int          Count;

		public ReadOnlyAsset(Asset asset) {
			Id       = asset.Id;
			Isin     = asset.Isin;
			Currency = asset.Currency;
			RawName  = asset.RawName;
			Count    = asset.Count;
		}
	}
}