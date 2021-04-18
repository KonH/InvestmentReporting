namespace InvestmentReporting.State.Entity {
	public sealed class ReadOnlyAsset {
		public readonly AssetId      Id;
		public readonly AssetISIN    Isin;
		public readonly CurrencyCode Currency;
		public readonly int          Count;

		public ReadOnlyAsset(Asset asset) {
			Id       = asset.Id;
			Isin     = asset.Isin;
			Currency = asset.Currency;
			Count    = asset.Count;
		}
	}
}