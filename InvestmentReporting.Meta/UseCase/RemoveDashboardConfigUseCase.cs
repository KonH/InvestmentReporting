using System.Threading.Tasks;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class RemoveDashboardConfigUseCase {
		readonly DashboardManager _manager;

		public RemoveDashboardConfigUseCase(DashboardManager manager) {
			_manager = manager;
		}

		public async Task Handle(UserId user, DashboardId id) =>
			await _manager.RemoveDashboard(user, id);
	}
}