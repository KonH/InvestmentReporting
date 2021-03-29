using System;

namespace InvestmentReporting.Domain.Entity {
	public record Asset(
		AssetId Id, string Name, AssetCategory Category, AssetTicker Ticker,
		CurrencyId BoughtCurrency, decimal BoughtPrice, DateTimeOffset BoughtDate,
		int Count) {}
}