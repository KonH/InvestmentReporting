using System.ComponentModel.DataAnnotations;

namespace InvestmentReporting.StateService.Dto {
	public record BrokerDto([Required] string DisplayName) {}
}