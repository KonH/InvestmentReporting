using System.Collections.Generic;

namespace InvestmentReporting.Meta.Dto {
	public record DashboardStateTagDto(
		string Tag, DashboardAssetDto[] Assets, Dictionary<string, SumStateDto> Sums);
}