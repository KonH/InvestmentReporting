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
				.Select(m => m.Model)
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
		}

		public async Task AddOrUpdateDashboard(string user, DashboardConfigModel dashboard) {
			var id = new ObjectId(dashboard.Id);
			var model = (await _collection.FindAsync(m => m.Id == id)).FirstOrDefault();
			if ( model != null ) {
				model.Model = dashboard;
				await _collection.ReplaceOneAsync(m => m.Id == id, model);
			} else {
				model = new MongoDashboardConfigModel {
					User  = user,
					Model = dashboard
				};
				await _collection.InsertOneAsync(model);
			}
		}
	}
}