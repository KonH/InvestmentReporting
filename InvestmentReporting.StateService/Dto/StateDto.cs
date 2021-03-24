using System.ComponentModel.DataAnnotations;

namespace InvestmentReporting.StateService.Dto {
	public record StateDto([Required] BrokerDto[] Brokers) {}
}