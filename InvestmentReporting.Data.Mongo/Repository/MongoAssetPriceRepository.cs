using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetPriceRepository : IAssetPriceRepository {
		readonly ILogger _logger;

		readonly IMongoCollection<MongoAssetPriceModel> _collection;

		public MongoAssetPriceRepository(ILogger<MongoAssetPriceRepository> logger, IMongoDatabase database) {
			_logger     = logger;
			_collection = database.GetCollection<MongoAssetPriceModel>("marketPriceMetadata");
		}

		public async Task Add(AssetPriceModel price) {
			var model = new MongoAssetPriceModel {
				Price = price
			};
			await _collection.InsertOneAsync(model);
			_logger.LogTrace($"Price added: {price}");
		}

		public async Task Update(AssetPriceModel price) {
			var model = _collection.AsQueryable()
				.FirstOrDefault(m => (m.Price != null) && (m.Price.Isin == price.Isin));
			if ( model == null ) {
				_logger.LogWarning($"Price not found: {price}");
				return;
			}
			model.Price = price;
			await _collection.ReplaceOneAsync(m => m.Id == model.Id, model);
			_logger.LogTrace($"Price updated: {price}");
		}

		public IReadOnlyCollection<AssetPriceModel> GetAll() {
			var models = _collection.AsQueryable()
				.ToArray()
				.Select(m => m.Price)
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
			_logger.LogTrace($"Found {models.Length} prices");
			return models;
		}

		public async Task Clear() {
			await _collection.DeleteManyAsync(m => true);
			_logger.LogTrace("Prices cleared");
		}
	}
}