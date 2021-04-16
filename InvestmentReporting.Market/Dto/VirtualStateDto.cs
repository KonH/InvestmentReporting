using System.Collections.Generic;

namespace InvestmentReporting.Market.Dto {
	public record VirtualStateDto(Dictionary<string, CurrencyBalanceDto> Summary, VirtualBalanceDto[] Balances);
}