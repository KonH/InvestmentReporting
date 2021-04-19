using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
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
			_stateManager.Prepare(user);
			var report           = new XLWorkbook(stream);
			var incomeTransfers  = _moneyMoveParser.ReadIncomeTransfers(report);
			var expenseTransfers = _moneyMoveParser.ReadExpenseTransfers(report);
			var assets           = _assetParser.ReadAssets(report);
			var trades           = _tradeParser.ReadTrades(report, assets);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
					incomeTransfers.Select(t => t.Currency),
					expenseTransfers.Select(t => t.Currency),
					trades.Select(t => t.Currency),
					new[] { "RUB" })
				.Select(s => new CurrencyCode(s))
				.ToArray();
			var state  = _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new BrokerNotFoundException();
			}
			var currencyAccounts       = CreateCurrencyAccounts(requiredCurrencyCodes, broker.Accounts);
			var allIncomeCommands      = _stateManager.ReadCommands<AddIncomeCommand>(user, brokerId);
			var incomeAccountCommands  = CreateAccountCommands(currencyAccounts, allIncomeCommands);
			var allExpenseCommands     = _stateManager.ReadCommands<AddExpenseCommand>(user, brokerId);
			var expenseAccountCommands = CreateAccountCommands(currencyAccounts, allExpenseCommands);
			await FillIncomeTransfers(user, brokerId, incomeTransfers, currencyAccounts, incomeAccountCommands);
			await FillExpenseTransfers(user, brokerId, expenseTransfers, currencyAccounts, expenseAccountCommands);
			var addAssetCommands    = _stateManager.ReadCommands<AddAssetCommand>(user, brokerId).ToArray();
            var reduceAssetCommands = _stateManager.ReadCommands<ReduceAssetCommand>(user, brokerId).ToArray();
			await FillTrades(user, brokerId, trades, currencyAccounts, addAssetCommands, reduceAssetCommands);
			await _stateManager.Push();
		}

		async Task FillTrades(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Trade> trades,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			IReadOnlyCollection<AddAssetCommand> addCommands, IReadOnlyCollection<ReduceAssetCommand> reduceCommands) {
			var assetIds = new Dictionary<string, AssetId>();
			foreach ( var trade in trades ) {
				var date       = trade.Date;
				var isin       = trade.Isin;
				var count      = trade.Count;
				var price      = trade.Sum;
				var fee        = trade.Fee;
				var buy        = trade.Count > 0;
				var payAccount = currencyAccounts[new(trade.Currency)];
				var feeAccount = currencyAccounts[new("RUB")];
				if ( buy ) {
					if ( IsAlreadyPresent(date, isin, count, addCommands) ) {
						continue;
					}
					var assetId = await _buyAssetUseCase.Handle(
						date, user, brokerId, payAccount, feeAccount, new(isin), price, fee, trade.Name, count);
					assetIds[isin] = assetId;
				} else {
					var allAssetIds = addCommands
						.Where(add => (add.Isin == isin) && (add.Date <= date))
						.Select(add => add.Asset)
						.Distinct()
						.ToArray();
					var reduceCount = -count;
					if ( allAssetIds.Any(id => IsAlreadyPresent(date, new(id), reduceCount, reduceCommands)) ) {
						continue;
					}
					var assetId = assetIds[isin];
					await _sellAssetUseCase.Handle(date, user, brokerId, payAccount, feeAccount, assetId, price, fee, reduceCount);
				}
			}
		}
	}
}