using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Meta.Entity;

namespace InvestmentReporting.Meta.Logic {
	public sealed class DashboardManager {
		readonly IDashboardRepository _repository;

		public DashboardManager(IDashboardRepository repository) {
			_repository = repository;
		}

		public async Task<DashboardConfigState> GetConfig(UserId user) {
			var configs = await _repository.GetUserDashboardConfigs(user);
			var dashboards = configs
				.Select(m => new DashboardConfig(
					new(m.Id),
					m.Name,
					m.Tags
						.Select(t => new DashboardConfigTag(new(t.Tag), t.Target))
						.ToArray()))
				.ToArray();
			return new DashboardConfigState(dashboards);
		}

		public async Task UpdateDashboard(UserId user, DashboardConfig dashboard) {
			var dashboardModel = new DashboardConfigModel(
				dashboard.Id,
				dashboard.Name,
				dashboard.Tags
					.Select(t => new DashboardConfigTagModel(t.Tag, t.Target))
					.ToList());
			await _repository.AddOrUpdateDashboard(user, dashboardModel);
		}

		public async Task<DashboardState> GetState(UserId user, DashboardId dashboardId) {
			// TODO: implement completely
			var configs   = await _repository.GetUserDashboardConfigs(user);
			var dashboard = configs.First(c => c.Id == dashboardId);
			var assetSums = new Dictionary<CurrencyId, SumState> {
				[new("6073ef351b228eb052c2feae")] = new (10, 11)
			};
			var tags = dashboard.Tags
				.Select(tag => {
					var assets = new[] {
						new DashboardAsset(new("isin"), new($"{dashboardId} asset"), assetSums)
					};
					return new DashboardStateTag(tag.Tag, assets, assetSums);
				})
				.ToArray();
			return new(tags, assetSums);
		}
	}
}