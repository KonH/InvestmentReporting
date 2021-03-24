namespace InvestmentReporting.StateService.Dto {
	public record AccountDto(string Id, string Currency, string DisplayName, decimal Balance) {}
}