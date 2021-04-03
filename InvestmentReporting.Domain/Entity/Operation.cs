using System;

namespace InvestmentReporting.Domain.Entity {
	public record Operation(DateTimeOffset Date, OperationKind Kind, decimal Amount, string Category, AssetId? Asset) {}
}