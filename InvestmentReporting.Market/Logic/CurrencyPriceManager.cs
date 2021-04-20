using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.Market.Entity;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;

namespace InvestmentReporting.Market.Logic {
	public sealed class CurrencyPriceManager {
		readonly ILogger                  _logger;
		readonly IStateManager            _stateManager;
		readonly ICurrencyPriceRepository _repository;

		public CurrencyPriceManager(
			ILogger<CurrencyPriceManager> logger, IStateManager stateManager, ICurrencyPriceRepository repository) {
			_logger       = logger;
			_stateManager = stateManager;
			_repository   = repository;
		}

		CurrencyPriceModel? TryGetModel(AssetFIGI figi) =>
			_repository.GetAll()
				.FirstOrDefault(m => m.Figi == figi);

		CurrencyPriceModel? TryGetModel(CurrencyCode code) =>
			_repository.GetAll()
				.FirstOrDefault(m => string.Equals(m.Code, code, StringComparison.InvariantCultureIgnoreCase));

		public CurrencyPrice? TryGet(AssetFIGI figi) {
			var model = TryGetModel(figi);
			if ( model == null ) {
				return null;
			}
			return new CurrencyPrice(
				model.Code, model.Figi,
				model.Candles.Select(c => new Candle(c.Date, c.Open, c.Close, c.Low, c.High)).ToList());
		}

		public DateTime GetStartDate() {
			var firstCommand = _stateManager
				.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue)
				.FirstOrDefault(c => c.Date > DateTimeOffset.MinValue);
			return firstCommand?.Date.Date ?? DateTime.Today.AddDays(-1);
		}

		public async Task AddOrAppendCandles(CurrencyCode currency, AssetFIGI figi, CandleList candles) {
			var model = TryGetModel(figi);
			var candleModels = candles.Candles
				.Select(c => new CandleModel(new DateTimeOffset(c.Time), c.Open, c.Close, c.Low, c.High))
				.ToList();
			if ( model != null ) {
				_logger.LogTrace($"Add {candleModels.Count} candles to existing model '{currency}' with FIGI '{figi}'");
				model.Candles.AddRange(candleModels);
				await _repository.Update(model);
			} else {
				_logger.LogTrace($"Creating new model '{currency}' with FIGI '{figi}'");
				var newModel = new CurrencyPriceModel(currency, candles.Figi, candleModels);
				await _repository.Add(newModel);
			}
		}

		public decimal GetPriceAt(CurrencyCode from, CurrencyCode to, DateTimeOffset date) {
			_logger.LogTrace($"Get price from '{from}' to '{to}'");
			if ( from != "RUB" ) {
				var price = GetPriceAt(from, date);
				_logger.LogTrace($"Direct price from '{from}' to '{to}' is {price}");
				return price;
			}
			var priceDiv = GetPriceAt(to, date);
			if ( priceDiv == 0 ) {
				return 0;
			}
			var inversePrice = 1m / priceDiv;
			_logger.LogTrace($"Inverse price from '{from}' to '{to}' is {inversePrice}");
			return inversePrice;
		}

		decimal GetPriceAt(CurrencyCode currency, DateTimeOffset date) {
			var model = TryGetModel(currency);
			if ( model == null ) {
				_logger.LogError($"Failed to find model for '{currency}'");
				return 0;
			}
			var lastCandleBeforeDate = model.Candles
				.LastOrDefault(c => c.Date < date);
			_logger.LogTrace($"Last candle value for '{currency}' before '{date}' is {lastCandleBeforeDate}");
			return lastCandleBeforeDate?.Close ?? 0;
		}

		public async Task Reset() => await _repository.Clear();
	}
}