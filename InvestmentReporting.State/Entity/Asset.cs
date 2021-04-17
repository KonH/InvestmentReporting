namespace InvestmentReporting.State.Entity {
	public sealed class Asset {
		public readonly AssetId   Id;
		public readonly AssetISIN Isin;

		public int Count;

		public Asset(AssetId id, AssetISIN isin, int count) {
			Id    = id;
			Isin  = isin;
			Count = count;
		}
	}
}