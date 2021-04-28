using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetTagRepository : IAssetTagRepository {
		readonly ILogger _logger;

		readonly IMongoCollection<MongoUserAssetTagsModel> _collection;

		public MongoAssetTagRepository(ILogger<MongoAssetTagRepository> logger, IMongoDatabase database) {
			_logger     = logger;
			_collection = database.GetCollection<MongoUserAssetTagsModel>("assetTags");
		}

		public async Task<UserAssetTagsModel?> Get(string user) {
			var model = (await _collection.FindAsync(m => m.User == user)).FirstOrDefault();
			_logger.LogTrace($"Assets tags for user: {model}");
			return (model != null) ? new UserAssetTagsModel(model.User, model.Tags) : null;
		}

		public async Task Add(UserAssetTagsModel model) {
			var mongoModel = new MongoUserAssetTagsModel {
				User = model.User,
				Tags = model.Tags
			};
			await _collection.InsertOneAsync(mongoModel);
			_logger.LogTrace($"Asset tags added: {model}");
		}

		public async Task Update(UserAssetTagsModel model) {
			var mongoModel = (await _collection.FindAsync(m => m.User == model.User)).FirstOrDefault();
			mongoModel.Tags = model.Tags;
			await _collection.FindOneAndReplaceAsync(m => m.User == model.User, mongoModel);
			_logger.LogTrace($"Assets tags updated: {model}");
		}
	}
}