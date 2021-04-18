using System;

namespace InvestmentReporting.State.Entity {
	public record Operation(
		DateTimeOffset Date, OperationKind Kind, CurrencyCode Currency, decimal Amount, string Category,
		BrokerId Broker, AccountId Account, AssetId? Asset);
}