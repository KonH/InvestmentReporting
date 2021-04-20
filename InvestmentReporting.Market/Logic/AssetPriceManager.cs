using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.Market.Entity;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;

namespace InvestmentReporting.Market.Logic {
	public sealed class AssetPriceManager {
		readonly ILogger               _logger;
		readonly IAssetPriceRepository _repository;
		readonly IStateManager         _stateManager;

		public AssetPriceManager(
			ILogger<AssetPriceManager> logger, IAssetPriceRepository repository, IStateManager stateManager) {
			_logger       = logger;
			_repository   = repository;
			_stateManager = stateManager;
		}

		AssetPriceModel? TryGetModel(AssetISIN isin) =>
			_repository.GetAll()
				.FirstOrDefault(m => m.Isin == isin);

		public VirtualBalance GetVirtualBalance(CurrencyCode currency, IReadOnlyCollection<VirtualAsset> inventory) {
			var assets = inventory
				.Where(a => a.Currency == currency)
				.ToArray();
			var assetRealSum = assets.Sum(a => a.RealSum);
			var assetVirtualSum = assets.Sum(a => a.VirtualSum);
			var inventoryForCurrency = inventory
				.Where(a => a.Currency == currency)
				.ToArray();
			return new VirtualBalance(
				RealSum: assetRealSum,
				VirtualSum: assetVirtualSum,
				inventoryForCurrency,
				currency);
		}

		public IEnumerable<AddAssetCommand> GetAddAssetCommands(AssetISIN isin) =>
			_stateManager.ReadCommands<AddAssetCommand>()
				.Where(c => c.Isin == isin);

		public DateTimeOffset GetLastBuyDate(UserId user, AssetId asset) {
			var buyCommands = _stateManager.ReadCommands<AddExpenseCommand>(user, asset)
				.Where(a => (a.Category == ExpenseCategory.BuyAsset))
				.ToArray();
			return (buyCommands.Length > 0)
				? buyCommands.Max(c => c.Date)
				: DateTimeOffset.MinValue;
		}

		public decimal GetRealPriceSum(DateTimeOffset date, UserId user, AssetId asset) {
			var assetIncomes = _stateManager.ReadCommands<AddIncomeCommand>(date, user, asset)
				.Where(a => (a.Category == IncomeCategory.SellAsset));
			var assetExpenses = _stateManager.ReadCommands<AddExpenseCommand>(date, user, asset)
				.Where(a => (a.Category == ExpenseCategory.BuyAsset));
			return assetExpenses.Sum(c => c.Amount) - assetIncomes.Sum(c => c.Amount);
		}

		public decimal? GetVirtualPricePerOne(AssetISIN isin, DateTimeOffset date) {
			var model = TryGetModel(isin);
			if ( model == null ) {
				_logger.LogWarning($"No model for ISIN '{isin}'");
				return null;
			}
			var lastCandleBeforeDate = model.Candles
				.LastOrDefault(c => c.Date < date);
			_logger.LogTrace($"Last candle value for ISIN '{isin}' before {date} is {lastCandleBeforeDate}");
			return lastCandleBeforeDate?.Close;
		}

		public decimal GetYearDividend(DateTimeOffset date, UserId user, AssetId asset) =>
			GetDividends(date.AddYears(-1), date, user, asset);

		public decimal GetDividendSum(DateTimeOffset date, UserId user, AssetId asset) =>
			GetDividends(DateTimeOffset.MinValue, date, user, asset);

		decimal GetDividends(DateTimeOffset startDate, DateTimeOffset endDate, UserId user, AssetId asset) {
			var dividendIncomes = _stateManager.ReadCommands<AddIncomeCommand>(startDate, endDate, user, asset)
				.Where(a => (a.Category == IncomeCategory.Dividend));
			return dividendIncomes.Aggregate(0m, (sum, a) => sum + a.Amount);
		}

		public AssetPrice? TryGet(AssetISIN isin) {
			var model = TryGetModel(isin);
			if ( model == null ) {
				return null;
			}
			return new AssetPrice(
				model.Isin, model.Figi,
				model.Candles.Select(c => new Candle(c.Date, c.Open, c.Close, c.Low, c.High)).ToList());
		}

		public async Task AddOrAppendCandles(AssetISIN isin, CandleList candles) {
			var model = TryGetModel(isin);
			var candleModels = candles.Candles
				.Select(c => new CandleModel(new DateTimeOffset(c.Time), c.Open, c.Close, c.Low, c.High))
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

		public async Task Reset() => await _repository.Clear();
	}
}