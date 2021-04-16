using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetTagRepository : IAssetTagRepository {
		public MongoAssetTagRepository(IMongoDatabase database) {}

		public UserAssetTagsModel? Get(string user) {
			throw new System.NotImplementedException();
		}

		public Task Add(string user, string asset, string tag) {
			throw new System.NotImplementedException();
		}

		public Task Remove(string user, string asset, string tag) {
			throw new System.NotImplementedException();
		}
	}
}