namespace InvestmentReporting.Domain.Entity {
	public sealed class Asset {
		public readonly AssetId       Id;
		public readonly string        Name;
		public readonly AssetCategory Category;
		public readonly AssetISIN     Isin;

		public int Count;

		public Asset(AssetId id, string name, AssetCategory category, AssetISIN isin, int count) {
			Id       = id;
			Name     = name;
			Category = category;
			Isin     = isin;
			Count    = count;
		}
	}
}