using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoDashboardRepository : IDashboardRepository {
		readonly IMongoCollection<MongoDashboardConfigModel> _collection;

		public MongoDashboardRepository(IMongoDatabase database) {
			_collection = database.GetCollection<MongoDashboardConfigModel>("dashboardConfigs");
		}

		public async Task<IReadOnlyCollection<DashboardConfigModel>> GetUserDashboardConfigs(string user) {
			var result = await _collection.FindAsync(m => m.User == user);
			return result.ToEnumerable()
				.ToArray()
				.Select(m => (m.Model! with { Id = m.Id.ToString() }))
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
		}

		public async Task AddOrUpdateDashboard(string user, DashboardConfigModel dashboard) {
			if ( !string.IsNullOrEmpty(dashboard.Id) ) {
				var id    = new ObjectId(dashboard.Id);
				var model = (await _collection.FindAsync(m => m.Id == id)).FirstOrDefault();
				if ( model != null ) {
					model.Model = dashboard;
					await _collection.ReplaceOneAsync(m => m.Id == id, model);
					return;
				}
			}
			var newModel = new MongoDashboardConfigModel {
				User  = user,
				Model = dashboard
			};
			await _collection.InsertOneAsync(newModel);
		}

		public async Task RemoveDashboard(string user, string id) {
			var objectId = new ObjectId(id);
			await _collection.DeleteOneAsync(m => m.Id == objectId);
		}
	}
}