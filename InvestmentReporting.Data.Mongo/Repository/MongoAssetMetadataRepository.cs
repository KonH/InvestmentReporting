using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetMetadataRepository : IAssetMetadataRepository {
		readonly ILogger _logger;

		readonly IMongoCollection<MongoAssetMetadataModel> _collection;

		public MongoAssetMetadataRepository(ILogger<MongoAssetMetadataRepository> logger, IMongoDatabase database) {
			_logger     = logger;
			_collection = database.GetCollection<MongoAssetMetadataModel>("marketAssetMetadata");
		}

		public async Task Add(AssetMetadataModel metadata) {
			var model = new MongoAssetMetadataModel {
				Metadata = metadata
			};
			await _collection.InsertOneAsync(model);
			_logger.LogTrace($"Metadata added: {model}");
		}

		public IReadOnlyCollection<AssetMetadataModel> GetAll() {
			var models = _collection.AsQueryable()
				.ToArray()
				.Select(m => m.Metadata)
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
			_logger.LogTrace($"Found metadata of {models.Length} assets");
			return models;
		}
	}
}