namespace InvestmentReporting.Domain.Entity {
	public sealed class Asset {
		public readonly AssetId       Id;
		public readonly string        Name;
		public readonly AssetCategory Category;
		public readonly AssetTicker   Ticker;

		public int Count;

		public Asset(AssetId id, string name, AssetCategory category, AssetTicker ticker, int count) {
			Id       = id;
			Name     = name;
			Category = category;
			Ticker   = ticker;
			Count    = count;
		}
	}
}