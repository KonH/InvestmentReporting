using System;

namespace InvestmentReporting.Domain.Entity {
	public record Asset(
		AssetId Id, string Name, AssetCategory Category, AssetTicker Ticker,
		CurrencyId BoughtCurrency, decimal BoughtPrice, CurrencyId FeeCurrency, decimal FeePrice,
		DateTimeOffset BoughtDate, int Count) {}
}