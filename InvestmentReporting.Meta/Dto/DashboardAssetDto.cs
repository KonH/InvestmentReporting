using System.Collections.Generic;

namespace InvestmentReporting.Meta.Dto {
	public record DashboardAssetDto(string Isin, string Name, Dictionary<string, SumStateDto> Sums);
}