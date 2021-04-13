using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Market.Entity;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;

namespace InvestmentReporting.Market.Logic {
	public sealed class PriceManager {
		readonly ILogger               _logger;
		readonly IAssetPriceRepository _repository;
		readonly IStateManager         _stateManager;

		public PriceManager(ILogger<PriceManager> logger, IAssetPriceRepository repository, IStateManager stateManager) {
			_logger       = logger;
			_repository   = repository;
			_stateManager = stateManager;
		}

		AssetPriceModel? TryGetModel(AssetISIN isin) =>
			_repository.GetAll()
				.FirstOrDefault(m => m.Isin == isin);

		IReadOnlyCollection<T> Filter<T>(DateTimeOffset date) where T : class, ICommandModel =>
			_stateManager.ReadCommands(DateTimeOffset.MinValue, date)
				.Select(c => c as T)
				.Where(c => c != null)
				.Select(c => c!)
				.ToArray();

		public IReadOnlyCollection<AddAssetModel> GetAddAssetCommands(AssetISIN isin, DateTimeOffset date) {
			return Filter<AddAssetModel>(date)
				.Where(c => c.Isin == isin)
				.ToArray();
		}

		public CurrencyId GetCurrency(AssetId asset, UserId user, BrokerId broker) {
			var assetBuy = Filter<AddExpenseModel>(DateTimeOffset.MaxValue)
				.First(a => (a.Category == "Asset Buy") && (a.Asset == asset));
			var accountId = assetBuy.Account;
			var state = _stateManager.ReadState(DateTimeOffset.MaxValue, user);
			return state.Brokers.First(b => b.Id == broker)
				.Accounts.First(a => a.Id == accountId)
				.Currency;
		}

		public decimal GetRealPriceSum(AssetId asset, DateTimeOffset date) {
			var assetIncomes = Filter<AddIncomeModel>(date)
				.Where(a => (a.Category == "Asset Sell") && (a.Asset == asset));
			var assetExpenses = Filter<AddExpenseModel>(date)
				.Where(a => (a.Category == "Asset Buy") && (a.Asset == asset));
			return assetExpenses.Sum(c => c.Amount) - assetIncomes.Sum(c => c.Amount);
		}

		public decimal? GetVirtualPricePerOne(AssetISIN isin, DateTimeOffset date) {
			var model = TryGetModel(isin);
			if ( model == null ) {
				return null;
			}
			var lastCandleBeforeDate = model.Candles
				.LastOrDefault(c => c.Date < date);
			return lastCandleBeforeDate?.Close;
		}

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