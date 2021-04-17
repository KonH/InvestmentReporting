using System.Collections.Generic;

namespace InvestmentReporting.Meta.Dto {
	public record DashboardStateTagDto(DashboardAssetDto[] Assets, Dictionary<string, SumStateDto> Sums);
}