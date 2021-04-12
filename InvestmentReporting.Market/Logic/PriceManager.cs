using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Market.Entity;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;

namespace InvestmentReporting.Market.Logic {
	public sealed class PriceManager {
		readonly ILogger               _logger;
		readonly IAssetPriceRepository _repository;

		public PriceManager(ILogger<PriceManager> logger, IAssetPriceRepository repository) {
			_logger     = logger;
			_repository = repository;
		}

		AssetPriceModel? TryGetModel(AssetISIN isin) =>
			_repository.GetAll()
				.FirstOrDefault(m => m.Isin == isin);

		public AssetPrice? TryGet(AssetISIN isin) {
			var model = TryGetModel(isin);
			if ( model == null ) {
				return null;
			}
			return new AssetPrice(
				model.Isin, model.Figi,
				model.Candles.Select(c => new AssetCandle(c.Date, c.Open, c.Close, c.Low, c.High)).ToList());
		}

		public async Task AddOrAppendCandles(AssetISIN isin, CandleList candles) {
			var model = TryGetModel(isin);
			var candleModels = candles.Candles
				.Select(c => new AssetCandleModel(new DateTimeOffset(c.Time), c.Open, c.Close, c.Low, c.High))
				.ToList();
			if ( model != null ) {
				_logger.LogTrace($"Add {candleModels.Count} candles to existing model with ISIN '{isin}'");
				model.Candles.AddRange(candleModels);
				await _repository.Update(model);
			} else {
				_logger.LogTrace($"Creating new model with ISIN '{isin}'");
				var newModel = new AssetPriceModel(isin, candles.Figi, candleModels);
				await _repository.Add(newModel);
			}
		}
	}
}