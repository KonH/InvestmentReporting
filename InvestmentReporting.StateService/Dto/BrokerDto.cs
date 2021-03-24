namespace InvestmentReporting.StateService.Dto {
	public record BrokerDto(string Id, string DisplayName, AccountDto[] Accounts) {}
}