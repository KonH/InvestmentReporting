using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class UpdateDashboardConfigUseCase {
		readonly DashboardManager _manager;

		public UpdateDashboardConfigUseCase(DashboardManager manager) {
			_manager = manager;
		}

		public async Task Handle(UserId user, DashboardConfig dashboard) =>
			await _manager.UpdateDashboard(user, dashboard);
	}
}