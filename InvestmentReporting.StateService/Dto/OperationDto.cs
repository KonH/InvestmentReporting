using System;

namespace InvestmentReporting.StateService.Dto {
	public record OperationDto(
		DateTimeOffset Date, string Kind, string Currency, decimal Amount, string Category, string? Asset);
}