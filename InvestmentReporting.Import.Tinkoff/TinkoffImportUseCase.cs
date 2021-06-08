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

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class TinkoffImportUseCase : ImportUseCase, IImportUseCase {
		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly AssetParser             _assetParser;
		readonly TradeParser             _tradeParser;
		readonly CouponParser            _couponParser;
		readonly BuyAssetUseCase         _buyAssetUseCase;
		readonly SellAssetUseCase        _sellAssetUseCase;

		public TinkoffImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser,
			AssetParser assetParser, TradeParser tradeParser, CouponParser couponParser,
			AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase,
			BuyAssetUseCase buyAssetUseCase, SellAssetUseCase sellAssetUseCase) : base(addIncomeUseCase, addExpenseUseCase) {
			_stateManager     = stateManager;
			_moneyMoveParser  = moneyMoveParser;
			_assetParser      = assetParser;
			_tradeParser      = tradeParser;
			_couponParser     = couponParser;
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
			var exchanges        = _tradeParser.ReadExchanges(report);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
					incomeTransfers.Select(t => t.Currency),
					expenseTransfers.Select(t => t.Currency),
					trades.Select(t => t.Currency),
					exchanges.Select(e => e.FromCurrency),
					exchanges.Select(e => e.ToCurrency),
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
			await FillIncomeTransfers(user, brokerId, incomeTransfers, exchanges, currencyAccounts, incomeAccountCommands);
			await FillExpenseTransfers(user, brokerId, expenseTransfers, exchanges, currencyAccounts, expenseAccountCommands);
			var addAssetCommands    = _stateManager.ReadCommands<AddAssetCommand>(user, brokerId).ToArray();
            var reduceAssetCommands = _stateManager.ReadCommands<ReduceAssetCommand>(user, brokerId).ToArray();
			var assetIds = await FillTrades(user, brokerId, trades, currencyAccounts, addAssetCommands, reduceAssetCommands);
			var couponTransfers = _moneyMoveParser.ReadCouponTransfers(report);
			await FillCoupons(user, brokerId, couponTransfers, currencyAccounts, incomeAccountCommands, trades, assetIds);
			var redemptionTransfers = _moneyMoveParser.ReadRedemptionTransfers(report);
			await FillRedemptions(user, brokerId, redemptionTransfers, currencyAccounts, incomeAccountCommands, trades, assetIds);
			await _stateManager.Push();
		}

		async Task<Dictionary<string, AssetId>> FillTrades(
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
			return assetIds;
		}

		async Task FillCoupons(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> couponTransfers,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			Dictionary<AccountId, IReadOnlyCollection<AddIncomeCommand>> incomeAccountCommands,
			IReadOnlyCollection<Trade> trades, IReadOnlyDictionary<string, AssetId> assets) {
			foreach ( var couponTransfer in couponTransfers ) {
				var accountId = currencyAccounts[new(couponTransfer.Currency)];
				if ( IsAlreadyPresent(couponTransfer.Date, couponTransfer.Amount, incomeAccountCommands[accountId]) ) {
					continue;
				}
				var asset = _couponParser.DetectAssetFromTransfer(couponTransfer.Comment, trades, assets);
				await AddIncomeUseCase.Handle(
					couponTransfer.Date, user, brokerId, accountId, couponTransfer.Amount,
					IncomeCategory.Coupon, asset);
			}
		}

		async Task FillRedemptions(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> redemptionTransfers,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			Dictionary<AccountId, IReadOnlyCollection<AddIncomeCommand>> incomeAccountCommands,
			IReadOnlyCollection<Trade> trades, Dictionary<string, AssetId> assets) {
			foreach ( var couponTransfer in redemptionTransfers ) {
				var accountId = currencyAccounts[new(couponTransfer.Currency)];
				if ( IsAlreadyPresent(couponTransfer.Date, couponTransfer.Amount, incomeAccountCommands[accountId]) ) {
					continue;
				}
				var asset = _couponParser.DetectAssetFromTransfer(couponTransfer.Comment, trades, assets);
				await AddIncomeUseCase.Handle(
					couponTransfer.Date, user, brokerId, accountId, couponTransfer.Amount,
					IncomeCategory.Coupon, asset);
			}
		}
	}
}