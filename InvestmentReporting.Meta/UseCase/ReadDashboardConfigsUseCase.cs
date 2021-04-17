using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.Logic;

namespace InvestmentReporting.Meta.UseCase {
	public sealed class ReadDashboardConfigsUseCase {
		readonly DashboardManager _manager;

		public ReadDashboardConfigsUseCase(DashboardManager manager) {
			_manager = manager;
		}

		public async Task<DashboardConfigState> Handle(UserId user) => await _manager.GetConfig(user);
	}
}