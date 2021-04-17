using System.Collections.Generic;

namespace InvestmentReporting.Meta.Entity {
	public record DashboardConfigState(IReadOnlyCollection<DashboardConfig> Dashboards);
}