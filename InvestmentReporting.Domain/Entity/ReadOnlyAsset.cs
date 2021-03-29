using System;

namespace InvestmentReporting.Domain.Entity {
	public sealed class ReadOnlyAsset {
		public readonly AssetId        Id;
		public readonly string         Name;
		public readonly AssetCategory  Category;
		public readonly AssetTicker    Ticker;
		public readonly CurrencyId     BoughtCurrency;
		public readonly decimal        BoughtPrice;
		public readonly DateTimeOffset BoughtDate;
		public readonly int            Count;

		public ReadOnlyAsset(Asset asset) {
			Id             = asset.Id;
			Name           = asset.Name;
			Category       = asset.Category;
			Ticker         = asset.Ticker;
			BoughtCurrency = asset.BoughtCurrency;
			BoughtPrice    = asset.BoughtPrice;
			BoughtDate     = asset.BoughtDate;
			Count          = asset.Count;
		}
	}
}