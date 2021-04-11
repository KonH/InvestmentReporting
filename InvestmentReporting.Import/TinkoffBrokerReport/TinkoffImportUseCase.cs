using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;
using InvestmentReporting.Import.UseCase;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class TinkoffImportUseCase : ImportUseCase, IImportUseCase {
		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly AssetParser             _assetParser;
		readonly TradeParser             _tradeParser;
		readonly BuyAssetUseCase         _buyAssetUseCase;
		readonly SellAssetUseCase        _sellAssetUseCase;

		public TinkoffImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser,
			AssetParser assetParser, TradeParser tradeParser,
			AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase,
			BuyAssetUseCase buyAssetUseCase, SellAssetUseCase sellAssetUseCase) : base(addIncomeUseCase, addExpenseUseCase) {
			_stateManager     = stateManager;
			_moneyMoveParser  = moneyMoveParser;
			_assetParser      = assetParser;
			_tradeParser      = tradeParser;
			_buyAssetUseCase  = buyAssetUseCase;
			_sellAssetUseCase = sellAssetUseCase;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, Stream stream) {
			await _stateManager.Prepare(user);
			var report           = new XLWorkbook(stream);
			var incomeTransfers  = _moneyMoveParser.ReadIncomeTransfers(report);
			var expenseTransfers = _moneyMoveParser.ReadExpenseTransfers(report);
			var assets           = _assetParser.ReadAssets(report);
			var trades           = _tradeParser.ReadTrades(report, assets);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
				incomeTransfers.Select(t => t.Currency),
				expenseTransfers.Select(t => t.Currency),
				trades.Select(t => t.Currency),
				new [] { "RUB" });
			var state  = await _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new BrokerNotFoundException();
			}
			var currencyAccounts     = CreateCurrencyAccounts(requiredCurrencyCodes, state.Currencies, broker.Accounts);
			var allCommands          = await _stateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, user);
			var allIncomeModels      = Filter<AddIncomeModel>(allCommands);
			var incomeAccountModels  = CreateIncomeModels(currencyAccounts, allIncomeModels);
			var allExpenseModels     = Filter<AddExpenseModel>(allCommands);
			var expenseAccountModels = CreateExpenseModels(currencyAccounts, allExpenseModels);
			await FillIncomeTransfers(user, brokerId, incomeTransfers, currencyAccounts, incomeAccountModels);
			await FillExpenseTransfers(user, brokerId, expenseTransfers, currencyAccounts, expenseAccountModels);
			var addAssetModels    = Filter<AddAssetModel>(allCommands);
            var reduceAssetModels = Filter<ReduceAssetModel>(allCommands);
			await FillTrades(user, brokerId, trades, currencyAccounts, addAssetModels, reduceAssetModels);
			await _stateManager.Push();
		}

		async Task FillTrades(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Trade> trades,
			Dictionary<string, AccountId> currencyAccounts,
			AddAssetModel[] addModels, ReduceAssetModel[] reduceModels) {
			var assetIds = new Dictionary<string, AssetId>();
			foreach ( var trade in trades ) {
				var date       = trade.Date;
				var isin       = trade.Isin;
				var count      = trade.Count;
				var price      = trade.Sum;
				var fee        = trade.Fee;
				var buy        = trade.Count > 0;
				var payAccount = currencyAccounts[trade.Currency];
				var feeAccount = currencyAccounts["RUB"];
				if ( buy ) {
					if ( IsAlreadyPresent(date, isin, count, addModels) ) {
						continue;
					}
					var assetId = await _buyAssetUseCase.Handle(
						date, user, brokerId, payAccount, feeAccount, new(isin), price, fee, count);
					assetIds[isin] = assetId;
				} else {
					var allAssetIds = addModels
						.Where(add => (add.Isin == isin) && (add.Date <= date))
						.Select(add => add.Id)
						.Distinct()
						.ToArray();
					var reduceCount = -count;
					if ( allAssetIds.Any(id => IsAlreadyPresent(date, new(id), reduceCount, reduceModels)) ) {
						continue;
					}
					var assetId = assetIds[isin];
					await _sellAssetUseCase.Handle(date, user, brokerId, payAccount, feeAccount, assetId, price, fee, reduceCount);
				}
			}
		}
	}
}