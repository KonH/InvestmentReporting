using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoAssetPriceRepository : IAssetPriceRepository {
		readonly IMongoCollection<MongoAssetPriceModel> _collection;

		public MongoAssetPriceRepository(IMongoDatabase database) {
			_collection = database.GetCollection<MongoAssetPriceModel>("marketPriceMetadata");
		}

		public async Task Add(AssetPriceModel price) {
			var model = new MongoAssetPriceModel {
				Price = price
			};
			await _collection.InsertOneAsync(model);
		}

		public async Task Update(AssetPriceModel price) {
			var model = _collection.AsQueryable()
				.FirstOrDefault(m => (m.Price != null) && (m.Price.Isin == price.Isin));
			if ( model == null ) {
				return;
			}
			model.Price = price;
			await _collection.ReplaceOneAsync(m => m.Id == model.Id, model);
		}

		public IReadOnlyCollection<AssetPriceModel> GetAll() {
			var models = _collection.AsQueryable()
				.ToArray()
				.Select(m => m.Price)
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
			return models;
		}
	}
}