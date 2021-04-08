using System;
using System.IO;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Import.Logic;
using InvestmentReporting.Import.UseCase;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class TinkoffImportUseCase : IImportUseCase {
		readonly IncomeCategory  _incomeTransferCategory  = new("Income Transfer");
		readonly IncomeCategory  _dividendCategory        = new("Share Dividend");
		readonly IncomeCategory  _couponCategory          = new("Bond Coupon");
		readonly ExpenseCategory _expenseTransferCategory = new("Expense Transfer");

		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly TradeParser             _tradeParser;
		readonly AddIncomeUseCase        _addIncomeUseCase;
		readonly AddExpenseUseCase       _addExpenseUseCase;
		readonly BuyAssetUseCase         _buyAssetUseCase;
		readonly SellAssetUseCase        _sellAssetUseCase;

		public TinkoffImportUseCase(
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

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, Stream stream) {
			await _stateManager.Prepare(user);
			await _stateManager.Push();
		}
	}
}