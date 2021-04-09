using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Logic;
using InvestmentReporting.Import.UseCase;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class TinkoffImportUseCase : ImportUseCase, IImportUseCase {
		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly TradeParser             _tradeParser;
		readonly AddExpenseUseCase       _addExpenseUseCase;
		readonly BuyAssetUseCase         _buyAssetUseCase;
		readonly SellAssetUseCase        _sellAssetUseCase;

		public TinkoffImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser, TradeParser tradeParser,
			AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase,
			BuyAssetUseCase buyAssetUseCase, SellAssetUseCase sellAssetUseCase) : base(addIncomeUseCase) {
			_stateManager      = stateManager;
			_moneyMoveParser   = moneyMoveParser;
			_tradeParser       = tradeParser;
			_addExpenseUseCase = addExpenseUseCase;
			_buyAssetUseCase   = buyAssetUseCase;
			_sellAssetUseCase  = sellAssetUseCase;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, Stream stream) {
			await _stateManager.Prepare(user);
			var report          = new XLWorkbook(stream);
			var incomeTransfers = _moneyMoveParser.ReadIncomeTransfers(report);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
				incomeTransfers.Select(t => t.Currency), new [] { "RUB" });
			var state  = await _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new BrokerNotFoundException();
			}
			var currencyAccounts    = CreateCurrencyAccounts(requiredCurrencyCodes, state.Currencies, broker.Accounts);
			var allCommands         = await _stateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, user);
			var allIncomeModels     = Filter<AddIncomeModel>(allCommands);
			var incomeAccountModels = CreateIncomeModels(currencyAccounts, allIncomeModels);
			await FillIncomeTransfers(user, brokerId, incomeTransfers, currencyAccounts, incomeAccountModels);
			await _stateManager.Push();
		}
	}
}