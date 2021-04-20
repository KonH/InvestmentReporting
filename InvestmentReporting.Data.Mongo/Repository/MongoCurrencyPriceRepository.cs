using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Model;
using MongoDB.Driver;

namespace InvestmentReporting.Data.Mongo.Repository {
	public sealed class MongoCurrencyPriceRepository : ICurrencyPriceRepository {
		readonly IMongoCollection<MongoCurrencyPriceModel> _collection;

		public MongoCurrencyPriceRepository(IMongoDatabase database) {
			_collection = database.GetCollection<MongoCurrencyPriceModel>("marketCurrencyPriceMetadata");
		}

		public async Task Add(CurrencyPriceModel price) {
			var model = new MongoCurrencyPriceModel {
				Price = price
			};
			await _collection.InsertOneAsync(model);
		}

		public async Task Update(CurrencyPriceModel price) {
			var model = _collection.AsQueryable()
				.FirstOrDefault(m => (m.Price != null) && (m.Price.Figi == price.Figi));
			if ( model == null ) {
				return;
			}
			model.Price = price;
			await _collection.ReplaceOneAsync(m => m.Id == model.Id, model);
		}

		public IReadOnlyCollection<CurrencyPriceModel> GetAll() {
			var models = _collection.AsQueryable()
				.ToArray()
				.Select(m => m.Price)
				.Where(m => m != null)
				.Select(m => m!)
				.ToArray();
			return models;
		}

		public async Task Clear() {
			await _collection.DeleteManyAsync(m => true);
		}
	}
}