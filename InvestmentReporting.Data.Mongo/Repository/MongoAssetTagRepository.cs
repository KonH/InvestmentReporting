using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetTagRepository : IAssetTagRepository {
		readonly IMongoCollection<MongoUserAssetTagsModel> _collection;

		public MongoAssetTagRepository(IMongoDatabase database) {
			_collection = database.GetCollection<MongoUserAssetTagsModel>("assetTags");
		}

		public async Task<UserAssetTagsModel?> Get(string user) {
			var model = (await _collection.FindAsync(m => m.User == user)).FirstOrDefault();
			return (model != null) ? new UserAssetTagsModel(model.User, model.Tags) : null;
		}

		public async Task Add(UserAssetTagsModel model) {
			var mongoModel = new MongoUserAssetTagsModel {
				User = model.User,
				Tags = model.Tags
			};
			await _collection.InsertOneAsync(mongoModel);
		}

		public async Task Update(UserAssetTagsModel model) {
			var mongoModel = (await _collection.FindAsync(m => m.User == model.User)).FirstOrDefault();
			mongoModel.Tags = model.Tags;
			await _collection.FindOneAndReplaceAsync(m => m.User == model.User, mongoModel);
		}
	}
}