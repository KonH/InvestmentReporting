using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;
using InvestmentReporting.Import.Logic;
using InvestmentReporting.Import.UseCase;

namespace InvestmentReporting.Import.AlphaDirect {
	public sealed class AlphaDirectImportUseCase : ImportUseCase, IImportUseCase {
		readonly XmlSanitizer _sanitizer = new();

		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly TradeParser             _tradeParser;
		readonly CouponParser            _couponParser;
		readonly BuyAssetUseCase         _buyAssetUseCase;
		readonly SellAssetUseCase        _sellAssetUseCase;

		// To receive ISIN from any string
		readonly Regex _dividendIsinRegex = new("([A-Z0-9]{12})");

		public AlphaDirectImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser, TradeParser tradeParser,
			CouponParser couponParser, AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase,
			BuyAssetUseCase buyAssetUseCase, SellAssetUseCase sellAssetUseCase) : base(addIncomeUseCase, addExpenseUseCase) {
			_stateManager     = stateManager;
			_moneyMoveParser  = moneyMoveParser;
			_tradeParser      = tradeParser;
			_couponParser     = couponParser;
			_buyAssetUseCase  = buyAssetUseCase;
			_sellAssetUseCase = sellAssetUseCase;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, Stream stream) {
			var report = LoadXml(stream);
			_stateManager.Prepare(user);
			report = _sanitizer.Sanitize(report);
			var state = _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new BrokerNotFoundException();
			}
			var incomeTransfers  = _moneyMoveParser.ReadIncomeTransfers(report);
			var expenseTransfers = _moneyMoveParser.ReadExpenseTransfers(report);
			var trades            = _tradeParser.ReadTrades(report);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
					incomeTransfers.Select(t => t.Currency), expenseTransfers.Select(t => t.Currency),
					trades.Select(t => t.Currency), new[] { "RUB" })
				.Select(s => new CurrencyCode(s))
				.ToArray();
			var currencyAccounts       = CreateCurrencyAccounts(requiredCurrencyCodes, broker.Accounts);
			var allIncomeCommands      = _stateManager.ReadCommands<AddIncomeCommand>(user, brokerId);
			var incomeAccountCommands  = CreateAccountCommands(currencyAccounts, allIncomeCommands);
			var allExpenseCommands     = _stateManager.ReadCommands<AddExpenseCommand>(user, brokerId);
			var expenseAccountCommands = CreateAccountCommands(currencyAccounts, allExpenseCommands);
			await FillIncomeTransfers(user, brokerId, incomeTransfers, currencyAccounts, incomeAccountCommands);
			await FillExpenseTransfers(user, brokerId, expenseTransfers, currencyAccounts, expenseAccountCommands);
			var addAssetCommands    = _stateManager.ReadCommands<AddAssetCommand>(user, brokerId).ToArray();
			var reduceAssetCommands = _stateManager.ReadCommands<ReduceAssetCommand>(user, brokerId).ToArray();
			var assets              = await FillTrades(user, brokerId, trades, currencyAccounts, addAssetCommands, reduceAssetCommands);
			var dividendTransfers   = _moneyMoveParser.ReadDividendTransfers(report);
			await FillDividends(user, brokerId, dividendTransfers, currencyAccounts, incomeAccountCommands, assets);
			var couponTransfers = _moneyMoveParser.ReadCouponTransfers(report);
			await FillCoupons(user, brokerId, couponTransfers, currencyAccounts, incomeAccountCommands, trades, assets);
			await _stateManager.Push();
		}

		static XmlDocument LoadXml(Stream stream) {
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(stream);
			return xmlDocument;
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

		async Task FillDividends(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> dividendTransfers,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			Dictionary<AccountId, IReadOnlyCollection<AddIncomeCommand>> incomeAccountCommands,
			Dictionary<string, AssetId> assets) {
			foreach ( var dividendTransfer in dividendTransfers ) {
				var accountId = currencyAccounts[new(dividendTransfer.Currency)];
				if ( IsAlreadyPresent(dividendTransfer.Date, dividendTransfer.Amount, incomeAccountCommands[accountId]) ) {
					continue;
				}
				var asset = DetectAssetFromDividend(dividendTransfer.Comment, assets);
				await AddIncomeUseCase.Handle(
					dividendTransfer.Date, user, brokerId, accountId, dividendTransfer.Amount,
					IncomeCategory.Dividend, asset);
			}
		}

		AssetId DetectAssetFromDividend(string comment, Dictionary<string, AssetId> assets) {
			var match = _dividendIsinRegex.Match(comment);
			if ( !match.Success ) {
				throw new UnexpectedFormatException($"Failed to detect ISIN from comment '{comment}'");
			}
			var isin = match.Value;
			if ( assets.TryGetValue(isin, out var assetId) ) {
				return assetId;
			}
			throw new InvalidOperationException($"Failed to find asset for ISIN '{isin}'");
		}

		async Task FillCoupons(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> couponTransfers,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			Dictionary<AccountId, IReadOnlyCollection<AddIncomeCommand>> incomeAccountCommands,
			IReadOnlyCollection<Trade> trades, Dictionary<string, AssetId> assets) {
			foreach ( var couponTransfer in couponTransfers ) {
				var accountId = currencyAccounts[new(couponTransfer.Currency)];
				if ( IsAlreadyPresent(couponTransfer.Date, couponTransfer.Amount, incomeAccountCommands[accountId]) ) {
					continue;
				}
				var asset = _couponParser.DetectAssetFromCoupon(couponTransfer.Comment, trades, assets);
				await AddIncomeUseCase.Handle(
					couponTransfer.Date, user, brokerId, accountId, couponTransfer.Amount,
					IncomeCategory.Coupon, asset);
			}
		}
	}
}