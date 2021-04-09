using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public abstract class ImportUseCase {
		protected readonly IncomeCategory  IncomeTransferCategory  = new("Income Transfer");
		protected readonly IncomeCategory  DividendCategory        = new("Share Dividend");
		protected readonly IncomeCategory  CouponCategory          = new("Bond Coupon");
		protected readonly ExpenseCategory ExpenseTransferCategory = new("Expense Transfer");

		protected readonly AddIncomeUseCase AddIncomeUseCase;

		public ImportUseCase(AddIncomeUseCase addIncomeUseCase) {
			AddIncomeUseCase = addIncomeUseCase;
		}

		protected T[] Filter<T>(IReadOnlyCollection<ICommandModel> allCommands)
			where T : class, ICommandModel =>
			allCommands
				.Select(c => c as T)
				.Where(c => c != null)
				.Select(c => c!)
				.ToArray();

		protected string[] GetRequiredCurrencyCodes(params IEnumerable<string>[] codes) =>
			codes.Aggregate(new List<string>(), (acc, cur) => {
					acc.AddRange(cur);
					return acc;
				})
				.Distinct()
				.ToArray();

		protected Dictionary<string, AccountId> CreateCurrencyAccounts(
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

		AccountId? GetAccountIdForCurrencyCode(
			string code,
			IReadOnlyCollection<ReadOnlyCurrency> currencies,
			IReadOnlyCollection<ReadOnlyAccount> accounts) {
			var currency = currencies.FirstOrDefault(c => c.Code == code);
			return (currency != null) ? accounts.FirstOrDefault(a => a.Currency == currency.Id)?.Id : null;
		}

		protected async Task FillIncomeTransfers(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> incomeTransfers,
			Dictionary<string, AccountId> currencyAccounts,
			Dictionary<AccountId, AddIncomeModel[]> incomeAccountModels) {
			foreach ( var incomeTransfer in incomeTransfers ) {
				var accountId = currencyAccounts[incomeTransfer.Currency];
				if ( IsAlreadyPresent(incomeTransfer.Date, incomeTransfer.Amount, incomeAccountModels[accountId]) ) {
					continue;
				}
				await AddIncomeUseCase.Handle(
					incomeTransfer.Date, user, brokerId, accountId, incomeTransfer.Amount,
					IncomeTransferCategory, asset: null);
			}
		}

		protected bool IsAlreadyPresent(DateTimeOffset date, decimal amount, AddIncomeModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Amount == amount));

		protected bool IsAlreadyPresent(DateTimeOffset date, decimal amount, AddExpenseModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Amount == amount));

		protected bool IsAlreadyPresent(DateTimeOffset date, string isin, int count, AddAssetModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Isin == isin) && (model.Count == count));

		protected bool IsAlreadyPresent(DateTimeOffset date, AssetId id, int count, ReduceAssetModel[] models) =>
			models
				.Any(model => (model.Date == date) && (model.Id == id) && (model.Count == count));

		protected Dictionary<AccountId, AddIncomeModel[]> CreateIncomeModels(
			Dictionary<string, AccountId> currencyAccounts, AddIncomeModel[] allIncomeModels) =>
			currencyAccounts.Values.ToDictionary(
				accountId => accountId,
				accountId => allIncomeModels
					.Where(m => m.Account == accountId)
					.ToArray());
	}
}