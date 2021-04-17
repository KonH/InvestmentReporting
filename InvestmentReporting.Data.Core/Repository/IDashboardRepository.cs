using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IDashboardRepository {
		Task<IReadOnlyCollection<DashboardConfigModel>> GetUserDashboardConfigs(string user);

		Task AddOrUpdateDashboard(string user, DashboardConfigModel dashboard);
	}
}