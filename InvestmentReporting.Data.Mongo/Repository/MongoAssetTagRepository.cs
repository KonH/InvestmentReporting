using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetTagRepository : IAssetTagRepository {
		readonly IMongoCollection<UserAssetTagsModel> _collection;

		public MongoAssetTagRepository(IMongoDatabase database) {
			_collection = database.GetCollection<UserAssetTagsModel>("assetTags");
		}

		public async Task<UserAssetTagsModel?> Get(string user) {
			return (await _collection.FindAsync(m => m.User == user)).FirstOrDefault();
		}

		public async Task Add(UserAssetTagsModel model) {
			await _collection.InsertOneAsync(model);
		}

		public async Task Update(UserAssetTagsModel model) {
			await _collection.FindOneAndReplaceAsync(m => m.User == model.User, model);
		}
	}
}