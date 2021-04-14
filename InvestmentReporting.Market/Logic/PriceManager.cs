using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
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

		public VirtualBalance GetVirtualBalance(
			DateTimeOffset date, UserId user, CurrencyId currency, IReadOnlyCollection<VirtualAsset> inventory) {
			var state = _stateManager.ReadState(date, user);
			var accounts = state.Brokers
				.SelectMany(b => b.Accounts)
				.Where(a => a.Currency == currency)
				.ToArray();
			var accountSum = (accounts.Length > 0) ? accounts.Sum(a => a.Balance) : 0;
			var assets = inventory
				.Where(a => a.Currency == currency)
				.ToArray();
			var assetRealSum = (assets.Length > 0) ? assets.Sum(a => a.RealPrice) : 0;
			var assetVirtualSum = (assets.Length > 0) ? assets.Sum(a => a.VirtualPrice) : 0;
			return new VirtualBalance(accountSum + assetRealSum, accountSum + assetVirtualSum, currency);
		}

		public IEnumerable<AddAssetCommand> GetAddAssetCommands(AssetISIN isin) =>
			_stateManager.ReadCommands<AddAssetCommand>()
				.Where(c => c.Isin == isin);

		public CurrencyId GetCurrency(UserId user, BrokerId broker, AssetId asset) {
			var assetBuy = _stateManager.ReadCommands<AddExpenseCommand>(user)
				.First(a => (a.Category == ExpenseCategory.BuyAsset) && (a.Asset == asset));
			var accountId = assetBuy.Account;
			var state = _stateManager.ReadState(user);
			return state.Brokers.First(b => b.Id == broker)
				.Accounts.First(a => a.Id == accountId)
				.Currency;
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