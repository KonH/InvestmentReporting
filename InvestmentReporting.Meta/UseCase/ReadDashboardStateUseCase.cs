using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class ReadDashboardStateUseCase {
		readonly DashboardManager _manager;

		public ReadDashboardStateUseCase(DashboardManager manager) {
			_manager = manager;
		}

		public DashboardState Handle(UserId user, DashboardId dashboard) =>
			_manager.GetState(user, dashboard);
	}
}