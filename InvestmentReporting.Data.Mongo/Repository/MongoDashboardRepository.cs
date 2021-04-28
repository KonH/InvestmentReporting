using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoDashboardRepository : IDashboardRepository {
		readonly ILogger _logger;

		readonly IMongoCollection<MongoDashboardConfigModel> _collection;

		public MongoDashboardRepository(ILogger<MongoDashboardRepository> logger, IMongoDatabase database) {
			_logger     = logger;
			_collection = database.GetCollection<MongoDashboardConfigModel>("dashboardConfigs");
		}

		public async Task<IReadOnlyCollection<DashboardConfigModel>> GetUserDashboardConfigs(string user) {
			var cursor = await _collection.FindAsync(m => m.User == user);
			var results = cursor.ToEnumerable()
				.ToArray()
				.Select(m => (m.Model! with { Id = m.Id.ToString() }))
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
			_logger.LogTrace($"Configs found: {results.Length}");
			return results;
		}

		public async Task AddOrUpdateDashboard(string user, DashboardConfigModel dashboard) {
			if ( !string.IsNullOrEmpty(dashboard.Id) ) {
				_logger.LogTrace($"Id '{dashboard.Id}' found, dashboard should be updated");
				var id    = new ObjectId(dashboard.Id);
				var model = (await _collection.FindAsync(m => m.Id == id)).FirstOrDefault();
				if ( model != null ) {
					_logger.LogTrace("Dashboard for given Id found");
					model.Model = dashboard;
					await _collection.ReplaceOneAsync(m => m.Id == id, model);
					return;
				}
			}
			_logger.LogTrace("New model will be created");
			var newModel = new MongoDashboardConfigModel {
				User  = user,
				Model = dashboard
			};
			await _collection.InsertOneAsync(newModel);
		}

		public async Task RemoveDashboard(string user, string id) {
			var objectId = new ObjectId(id);
			await _collection.DeleteOneAsync(m => m.Id == objectId);
			_logger.LogTrace($"Dashboard '{id}' for user '{user}' was removed");
		}
	}
}