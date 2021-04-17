using System.Collections.Generic;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardConfig(DashboardId Id, string Name, IReadOnlyCollection<DashboardConfigTag> Tags);
}