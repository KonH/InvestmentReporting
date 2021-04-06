namespace InvestmentReporting.Domain.Entity {
	public sealed class ReadOnlyAsset {
		public readonly AssetId       Id;
		public readonly string        Name;
		public readonly AssetCategory Category;
		public readonly AssetISIN     Isin;
		public readonly int           Count;

		public ReadOnlyAsset(Asset asset) {
			Id       = asset.Id;
			Name     = asset.Name;
			Category = asset.Category;
			Isin     = asset.Isin;
			Count    = asset.Count;
		}
	}
}