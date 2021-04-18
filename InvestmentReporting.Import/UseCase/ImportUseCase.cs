using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.UseCase {
	public abstract class ImportUseCase {
		protected readonly AddIncomeUseCase  AddIncomeUseCase;
		protected readonly AddExpenseUseCase AddExpenseUseCase;

		protected ImportUseCase(AddIncomeUseCase addIncomeUseCase, AddExpenseUseCase addExpenseUseCase) {
			AddIncomeUseCase  = addIncomeUseCase;
			AddExpenseUseCase = addExpenseUseCase;
		}

		protected string[] GetRequiredCurrencyCodes(params IEnumerable<string>[] codes) =>
			codes.Aggregate(new List<string>(), (acc, cur) => {
					acc.AddRange(cur);
					return acc;
				})
				.Distinct()
				.ToArray();

		protected Dictionary<CurrencyCode, AccountId> CreateCurrencyAccounts(
			CurrencyCode[] requiredCurrencyCodes, IReadOnlyCollection<ReadOnlyAccount> accounts) =>
			requiredCurrencyCodes
				.ToDictionary(
					currencyCode => currencyCode,
					currencyCode => {
						var account = accounts.FirstOrDefault(a => a.Currency == currencyCode);
						if ( account == null ) {
							throw new AccountNotFoundException();
						}
						return account.Id;
					});

		protected async Task FillIncomeTransfers(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> incomeTransfers,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			Dictionary<AccountId, IReadOnlyCollection<AddIncomeCommand>> incomeAccountCommands) {
			foreach ( var incomeTransfer in incomeTransfers ) {
				var accountId = currencyAccounts[new(incomeTransfer.Currency)];
				if ( IsAlreadyPresent(incomeTransfer.Date, incomeTransfer.Amount, incomeAccountCommands[accountId]) ) {
					continue;
				}
				await AddIncomeUseCase.Handle(
					incomeTransfer.Date, user, brokerId, accountId, incomeTransfer.Amount,
					IncomeCategory.Transfer, asset: null);
			}
		}

		protected async Task FillExpenseTransfers(
			UserId user, BrokerId brokerId, IReadOnlyCollection<Transfer> expenseTransfers,
			Dictionary<CurrencyCode, AccountId> currencyAccounts,
			Dictionary<AccountId, IReadOnlyCollection<AddExpenseCommand>> expenseAccountCommands) {
			foreach ( var expenseTransfer in expenseTransfers ) {
				var amount    = -expenseTransfer.Amount;
				var accountId = currencyAccounts[new(expenseTransfer.Currency)];
				if ( IsAlreadyPresent(expenseTransfer.Date, amount, expenseAccountCommands[accountId]) ) {
					continue;
				}
				await AddExpenseUseCase.Handle(
					expenseTransfer.Date, user, brokerId, accountId, amount,
					ExpenseCategory.Transfer, asset: null);
			}
		}

		protected bool IsAlreadyPresent(DateTimeOffset date, decimal amount, IReadOnlyCollection<AddIncomeCommand> commands) =>
			commands
				.Any(model => (model.Date == date) && (model.Amount == amount));

		protected bool IsAlreadyPresent(DateTimeOffset date, decimal amount, IReadOnlyCollection<AddExpenseCommand> commands) =>
			commands
				.Any(model => (model.Date == date) && (model.Amount == amount));

		protected bool IsAlreadyPresent(DateTimeOffset date, string isin, int count, IReadOnlyCollection<AddAssetCommand> commands) =>
			commands
				.Any(model => (model.Date == date) && (model.Isin == isin) && (model.Count == count));

		protected bool IsAlreadyPresent(DateTimeOffset date, AssetId id, int count, IReadOnlyCollection<ReduceAssetCommand> commands) =>
			commands
				.Any(model => (model.Date == date) && (model.Asset == id) && (model.Count == count));

		protected Dictionary<AccountId, IReadOnlyCollection<TCommand>> CreateAccountCommands<TCommand>(
			Dictionary<CurrencyCode, AccountId> currencyAccounts, IEnumerable<TCommand> accountCommands)
			where TCommand : IAccountCommand =>
			currencyAccounts.Values.ToDictionary(
				accountId => accountId,
				accountId => (IReadOnlyCollection<TCommand>)accountCommands
					.Where(m => m.Account == accountId)
					.ToArray());
	}
}