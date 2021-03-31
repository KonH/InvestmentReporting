namespace InvestmentReporting.Domain.Entity {
	public sealed class ReadOnlyAsset {
		public readonly AssetId       Id;
		public readonly string        Name;
		public readonly AssetCategory Category;
		public readonly AssetTicker   Ticker;
		public readonly int           Count;

		public ReadOnlyAsset(Asset asset) {
			Id       = asset.Id;
			Name     = asset.Name;
			Category = asset.Category;
			Ticker   = asset.Ticker;
			Count    = asset.Count;
		}
	}
}