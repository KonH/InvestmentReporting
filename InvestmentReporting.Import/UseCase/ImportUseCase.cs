using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Logic;

namespace InvestmentReporting.Import.UseCase {
	public sealed class ImportUseCase {
		readonly IncomeCategory _incomeTransferCategory   = new("Income Transfer");
		readonly ExpenseCategory _expenseTransferCategory = new("Expense Transfer");

		readonly XmlSanitizer _sanitizer = new();

		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly AddIncomeUseCase        _addIncomeUseCase;
		readonly AddExpenseUseCase       _addExpenseUseCase;

		public ImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser,
			AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase) {
			_stateManager      = stateManager;
			_moneyMoveParser   = moneyMoveParser;
			_addIncomeUseCase  = addIncomeUseCase;
			_addExpenseUseCase = addExpenseUseCase;
		}

		public async Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, XmlDocument report) {
			report = _sanitizer.Sanitize(report);
			var state = await _stateManager.ReadState(date, user);
			var broker = state.Brokers.FirstOrDefault(b => b.Id == brokerId);
			if ( broker == null ) {
				throw new BrokerNotFoundException();
			}
			var allCommands = await _stateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, user);
			var allIncomeModels = allCommands
				.Select(c => c as AddIncomeModel)
				.Where(c => c != null)
				.Select(c => c!)
				.ToArray();
			var allExpenseModels = allCommands
				.Select(c => c as AddExpenseModel)
				.Where(c => c != null)
				.Select(c => c!)
				.ToArray();
			var incomeTransfers       = _moneyMoveParser.ReadIncomeTransfers(report);
			var expenseTransfers      = _moneyMoveParser.ReadExpenseTransfers(report);
			var requiredCurrencyCodes = GetRequiredCurrencyCodes(
				incomeTransfers.Select(t => t.Currency), expenseTransfers.Select(t => t.Currency));
			var currencyAccounts     = new Dictionary<string, AccountId>();
			var incomeAccountModels  = new Dictionary<AccountId, AddIncomeModel[]>();
			var expenseAccountModels = new Dictionary<AccountId, AddExpenseModel[]>();
			foreach ( var currencyCode in requiredCurrencyCodes ) {
				var accountId = GetAccountIdForCurrencyCode(currencyCode, state.Currencies, broker.Accounts);
				if ( accountId == null ) {
					throw new AccountNotFoundException();
				}
				currencyAccounts.Add(currencyCode, accountId);
				var incomeModels = allIncomeModels
					.Where(m => m.Account == accountId)
					.ToArray();
				incomeAccountModels.Add(accountId, incomeModels);
				var expenseModels = allExpenseModels
					.Where(m => m.Account == accountId)
					.ToArray();
				expenseAccountModels.Add(accountId, expenseModels);
			}
			foreach ( var incomeTransfer in incomeTransfers ) {
				var accountId = currencyAccounts[incomeTransfer.Currency];
				if ( IsAlreadyPresent(incomeTransfer.Date, incomeTransfer.Amount, incomeAccountModels[accountId]) ) {
					continue;
				}
				await _addIncomeUseCase.Handle(
					incomeTransfer.Date, user, brokerId, accountId, incomeTransfer.Amount,
					_incomeTransferCategory, asset: null);
			}
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
			await _stateManager.Push();
		}

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
				.Where(model => model.Date == date)
				.Any(model => model.Amount == amount);

		bool IsAlreadyPresent(DateTimeOffset date, decimal amount, AddExpenseModel[] models) =>
			models
				.Where(model => model.Date == date)
				.Any(model => model.Amount == amount);
	}
}