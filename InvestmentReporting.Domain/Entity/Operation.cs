using System;

namespace InvestmentReporting.Domain.Entity {
	public record Operation(
		DateTimeOffset Date, OperationKind Kind, CurrencyId Currency, decimal Amount, string Category, AssetId? Asset);
}