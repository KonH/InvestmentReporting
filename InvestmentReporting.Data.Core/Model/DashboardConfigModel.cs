using System.Collections.Generic;

namespace InvestmentReporting.Data.Core {
	public record DashboardConfigModel(string Id, string Name, List<DashboardConfigTagModel> Tags);
}