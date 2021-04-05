using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;

namespace InvestmentReporting.Import.UseCase {
	public sealed class ImportUseCase {
		readonly IncomeCategory _incomeTransferCategory = new("Income Transfer");

		readonly XmlSanitizer _sanitizer = new();

		readonly TransactionStateManager _stateManager;
		readonly BrokerMoneyMoveParser   _moneyMoveParser;
		readonly AddIncomeUseCase        _addIncomeUseCase;

		public ImportUseCase(
			TransactionStateManager stateManager, BrokerMoneyMoveParser moneyMoveParser,
			AddIncomeUseCase addIncomeUseCase) {
			_stateManager     = stateManager;
			_moneyMoveParser  = moneyMoveParser;
			_addIncomeUseCase = addIncomeUseCase;
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
			var incomeTransfers       = _moneyMoveParser.ReadIncomeTransfers(report);
			var requiredCurrencyCodes = incomeTransfers.Select(t => t.Currency).Distinct().ToArray();
			var currencyAccounts      = new Dictionary<string, AccountId>();
			var incomeAccountModels   = new Dictionary<AccountId, AddIncomeModel[]>();
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
			}
			foreach ( var incomeTransfer in incomeTransfers ) {
				var accountId = currencyAccounts[incomeTransfer.Currency];
				if ( IsAlreadyPresent(incomeTransfer, incomeAccountModels[accountId]) ) {
					continue;
				}
				await _addIncomeUseCase.Handle(
					incomeTransfer.Date, user, brokerId, accountId, incomeTransfer.Amount,
					_incomeTransferCategory, asset: null);
			}
			await _stateManager.Push();
		}

		AccountId? GetAccountIdForCurrencyCode(
			string code,
			IReadOnlyCollection<ReadOnlyCurrency> currencies,
			IReadOnlyCollection<ReadOnlyAccount> accounts) {
			var currency = currencies.FirstOrDefault(c => c.Code == code);
			return (currency != null) ? accounts.FirstOrDefault(a => a.Currency == currency.Id)?.Id : null;
		}

		bool IsAlreadyPresent(IncomeTransfer incomeTransfer, AddIncomeModel[] incomeModels) =>
			incomeModels
				.Where(model => model.Date == incomeTransfer.Date)
				.Any(model => model.Amount == incomeTransfer.Amount);
	}
}