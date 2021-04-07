using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;
using InvestmentReporting.Import.Logic;

namespace InvestmentReporting.Import.UseCase {
	public sealed class ImportUseCase {
		readonly IncomeCategory  _incomeTransferCategory  = new("Income Transfer");
		readonly IncomeCategory  _dividendCategory        = new("Share Dividend");
		readonly IncomeCategory  _couponCategory          = new("Bond Coupon");
		readonly ExpenseCategory _expenseTransferCategory = new("Expense Transfer");

		readonly XmlSanitizer _sanitizer = new();

		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly TradeParser             _tradeParser;
		readonly AddIncomeUseCase        _addIncomeUseCase;
		readonly AddExpenseUseCase       _addExpenseUseCase;
		readonly BuyAssetUseCase         _buyAssetUseCase;
		readonly SellAssetUseCase        _sellAssetUseCase;

		// To receive ISIN from any string
		readonly Regex _dividendIsinRegex = new("(\\w{2}\\d{10})");

		// To receive organization name and series from coupon comment
		readonly Regex _couponRegex = new("\\(Облигации (.*) сери.*(\\d{4}-\\d{2})\\)");

		// To receive organization name and series from asset name
		readonly Regex _bondRegex = new("(.*) сери.*(\\d{4}-\\d{2})");

		public ImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser, TradeParser tradeParser,
			AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase,
			BuyAssetUseCase buyAssetUseCase, SellAssetUseCase sellAssetUseCase) {
			_stateManager      = stateManager;
			_moneyMoveParser   = moneyMoveParser;
			_tradeParser       = tradeParser;
			_addIncomeUseCase  = addIncomeUseCase;
			_addExpenseUseCase = addExpenseUseCase;
			_buyAssetUseCase   = buyAssetUseCase;
			_sellAssetUseCase  = sellAssetUseCase;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, XmlDocument report) {
			await _stateManager.Prepare(user);
			report = _sanitizer.Sanitize(report);
			var state = await _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new BrokerNotFoundException();
			}
			var incomeTransfers  = _moneyMoveParser.ReadIncomeTransfers(report);
			var expenseTransfers = _moneyMoveParser.ReadExpenseTransfers(report);
			var trades            = _tradeParser.ReadTrades(report);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
				incomeTransfers.Select(t => t.Currency), expenseTransfers.Select(t => t.Currency),
				trades.Select(t => t.Currency), new [] { "RUB" });
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
			var assets            = await FillTrades(user, brokerId, trades, currencyAccounts, addAssetModels, reduceAssetModels);
			var dividendTransfers = _moneyMoveParser.ReadDividendTransfers(report);
			await FillDividends(user, brokerId, dividendTransfers, currencyAccounts, incomeAccountModels, assets);
			var couponTransfers = _moneyMoveParser.ReadCouponTransfers(report);
			await FillCoupons(user, brokerId, couponTransfers, currencyAccounts, incomeAccountModels, trades, assets);
			await _stateManager.Push();
		}

		T[] Filter<T>(IReadOnlyCollection<ICommandModel> allCommands)
			where T : class, ICommandModel =>
			allCommands
				.Select(c => c as T)
				.Where(c => c != null)
				.Select(c => c!)
				.ToArray();

		async Task FillIncomeTransfers(
			UserId user, BrokerId brokerId, IReadOnlyCollection<IncomeTransfer> incomeTransfers,
			Dictionary<string, AccountId> currencyAccounts,
			Dictionary<AccountId, AddIncomeModel[]> incomeAccountModels) {
			foreach ( var incomeTransfer in incomeTransfers ) {
				var accountId = currencyAccounts[incomeTransfer.Currency];
				if ( IsAlreadyPresent(incomeTransfer.Date, incomeTransfer.Amount, incomeAccountModels[accountId]) ) {
					continue;
				}
				await _addIncomeUseCase.Handle(
					incomeTransfer.Date, user, brokerId, accountId, incomeTransfer.Amount,
					_incomeTransferCategory, asset: null);
			}
		}

		async Task FillExpenseTransfers(
			UserId user, BrokerId brokerId, IReadOnlyCollection<ExpenseTransfer> expenseTransfers,
			Dictionary<string, AccountId> currencyAccounts,
			Dictionary<AccountId, AddExpenseModel[]> expenseAccountModels) {
			foreach ( var expenseTransfer in expenseTransfers ) {
				var amount    = -expenseTransfer.Amount;
				var accountId = currencyAccounts[expenseTransfer.Currency];
				if ( IsAlreadyPresent(expenseTransfer.Date, amount, expenseAccountModels[accountId]) ) {
					continue;
				}
				await _addExpenseUseCase.Handle(
					expenseTransfer.Date, user, brokerId, accountId, amount,
					_expenseTransferCategory, asset: null);
			}
		}

		async Task<Dictionary<string, AssetId>> FillTrades(
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
					var name     = trade.Name;
					var category = new AssetCategory(trade.Category);
					var assetId = await _buyAssetUseCase.Handle(
						date, user, brokerId, payAccount, feeAccount, name, category, new(isin), price, fee, count);
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
			return assetIds;
		}

		async Task FillDividends(
			UserId user, BrokerId brokerId, IReadOnlyCollection<IncomeTransfer> dividendTransfers,
			Dictionary<string, AccountId> currencyAccounts, Dictionary<AccountId, AddIncomeModel[]> incomeAccountModels,
			Dictionary<string, AssetId> assets) {
			foreach ( var dividendTransfer in dividendTransfers ) {
				var accountId = currencyAccounts[dividendTransfer.Currency];
				if ( IsAlreadyPresent(dividendTransfer.Date, dividendTransfer.Amount, incomeAccountModels[accountId]) ) {
					continue;
				}
				var asset = DetectAssetFromDividend(dividendTransfer.Comment, assets);
				await _addIncomeUseCase.Handle(
					dividendTransfer.Date, user, brokerId, accountId, dividendTransfer.Amount,
					_dividendCategory, asset);
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
			UserId user, BrokerId brokerId, IReadOnlyCollection<IncomeTransfer> couponTransfers,
			Dictionary<string, AccountId> currencyAccounts, Dictionary<AccountId, AddIncomeModel[]> incomeAccountModels,
			IReadOnlyCollection<Trade> trades, Dictionary<string, AssetId> assets) {
			foreach ( var couponTransfer in couponTransfers ) {
				var accountId = currencyAccounts[couponTransfer.Currency];
				if ( IsAlreadyPresent(couponTransfer.Date, couponTransfer.Amount, incomeAccountModels[accountId]) ) {
					continue;
				}
				var asset = DetectAssetFromCoupon(couponTransfer.Comment, trades, assets);
				await _addIncomeUseCase.Handle(
					couponTransfer.Date, user, brokerId, accountId, couponTransfer.Amount,
					_couponCategory, asset);
			}
		}

		AssetId DetectAssetFromCoupon(string comment, IReadOnlyCollection<Trade> trades, Dictionary<string, AssetId> assets) {
			var couponMatch = _couponRegex.Match(comment);
			if ( !couponMatch.Success ) {
				throw new UnexpectedFormatException($"Failed to detect organization and/or series from comment '{comment}'");
			}
			var organization = couponMatch.Groups[1].Value.Trim();
			var series       = couponMatch.Groups[2].Value;
			foreach ( var trade in trades ) {
				var tradeMatch = _bondRegex.Match(trade.Name);
				if ( !tradeMatch.Success ) {
					continue;
				}
				var tradeOrganization = tradeMatch.Groups[1].Value;
				var tradeSeries       = tradeMatch.Groups[2].Value;
				if ( !tradeOrganization.StartsWith(organization) || (series != tradeSeries) ) {
					continue;
				}
				var isin = trade.Isin;
				if ( assets.TryGetValue(isin, out var assetId) ) {
					return assetId;
				}
				throw new InvalidOperationException($"Failed to find asset for ISIN '{isin}'");
			}
			throw new InvalidOperationException($"Failed to find asset for '{organization}' '{series}'");
		}

		Dictionary<string, AccountId> CreateCurrencyAccounts(
			string[] requiredCurrencyCodes, IReadOnlyCollection<ReadOnlyCurrency> currencies,
			IReadOnlyCollection<ReadOnlyAccount> accounts) =>
			requiredCurrencyCodes
				.ToDictionary(
					currencyCode => currencyCode,
					currencyCode => {
						var accountId = GetAccountIdForCurrencyCode(currencyCode, currencies, accounts);
						if ( accountId == null ) {
							throw new AccountNotFoundException();
						}
						return accountId;
					});

		static Dictionary<AccountId, AddIncomeModel[]> CreateIncomeModels(
			Dictionary<string, AccountId> currencyAccounts, AddIncomeModel[] allIncomeModels) =>
			currencyAccounts.Values.ToDictionary(
				accountId => accountId,
				accountId => allIncomeModels
					.Where(m => m.Account == accountId)
					.ToArray());

		static Dictionary<AccountId, AddExpenseModel[]> CreateExpenseModels(
			Dictionary<string, AccountId> currencyAccounts, AddExpenseModel[] allExpenseModels) =>
			currencyAccounts.Values.ToDictionary(
				accountId => accountId,
				accountId => allExpenseModels
					.Where(m => m.Account == accountId)
					.ToArray());

		string[] GetRequiredCurrencyCodes(params IEnumerable<string>[] codes) =>
			codes.Aggregate(new List<string>(), (acc, cur) => {
					acc.AddRange(cur);
					return acc;
				})
				.Distinct()
				.ToArray();

		AccountId? GetAccountIdForCurrencyCode(
			string code,
			IReadOnlyCollection<ReadOnlyCurrency> currencies,
			IReadOnlyCollection<ReadOnlyAccount> accounts) {
			var currency = currencies.FirstOrDefault(c => c.Code == code);
			return (currency != null) ? accounts.FirstOrDefault(a => a.Currency == currency.Id)?.Id : null;
		}

		bool IsAlreadyPresent(DateTimeOffset date, decimal amount, AddIncomeModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Amount == amount));

		bool IsAlreadyPresent(DateTimeOffset date, decimal amount, AddExpenseModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Amount == amount));

		bool IsAlreadyPresent(DateTimeOffset date, string isin, int count, AddAssetModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Isin == isin) && (model.Count == count));

		bool IsAlreadyPresent(DateTimeOffset date, AssetId id, int count, ReduceAssetModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Id == id) && (model.Count == count));
	}
}