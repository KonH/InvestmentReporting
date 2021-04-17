using System.Collections.Generic;

namespace InvestmentReporting.Meta.Dto {
	public record DashboardStateDto(DashboardStateTagDto[] Tags, Dictionary<string, SumStateDto> Sums);
}