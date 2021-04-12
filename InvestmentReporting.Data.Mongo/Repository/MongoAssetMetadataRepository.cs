using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetMetadataRepository : IAssetMetadataRepository {
		readonly IMongoCollection<MongoAssetMetadataModel> _collection;

		public MongoAssetMetadataRepository(IMongoDatabase database) {
			_collection = database.GetCollection<MongoAssetMetadataModel>("marketAssetMetadata");
		}

		public async Task Add(AssetMetadataModel metadata) {
			var model = new MongoAssetMetadataModel {
				Metadata = metadata
			};
			await _collection.InsertOneAsync(model);
		}

		public IReadOnlyCollection<AssetMetadataModel> GetAll() {
			var models = _collection.AsQueryable()
				.ToArray()
				.Select(m => m.Metadata)
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
			return models;
		}
	}
}