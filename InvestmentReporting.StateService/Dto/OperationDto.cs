using System;

namespace InvestmentReporting.StateService.Dto {
	public record OperationDto(DateTimeOffset Date, string Kind, decimal Amount, string Category) {}
}