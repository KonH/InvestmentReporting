using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class CashOperationTests {
		readonly DateTimeOffset  _date            = DateTimeOffset.MinValue;
		readonly UserId          _userId          = new("user");
		readonly BrokerId        _brokerId        = new("broker");
		readonly CurrencyId      _currencyId      = new("currency");
		readonly AccountId       _accountId       = new("account");
		readonly IncomeCategory  _incomeCategory  = new("income_category");
		readonly ExpenseCategory _expenseCategory = new("expense_category");
		readonly AssetId         _assetId         = new("asset");

		[Test]
		public async Task IsIncomeAdded() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddIncomeUseCase(stateManager, new GuidIdGenerator());

			await addUseCase.Handle(_date, _userId, _brokerId, _accountId, 100, _incomeCategory, _assetId);

			var state   = await stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _accountId);
			account.Balance.Should().Be(100);
		}

		[Test]
		public void IsIncomeForUnknownBrokerFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddIncomeUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<BrokerNotFoundException>(() => addUseCase.Handle(
				_date, _userId, new(string.Empty), _accountId, 100, _incomeCategory, null));
		}

		[Test]
		public void IsIncomeForUnknownAccountFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddIncomeUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<AccountNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, new(string.Empty), 100, _incomeCategory, null));
		}

		[Test]
		public void IsIncomeWithEmptyCategoryFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddIncomeUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidCategoryException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, 100, new(string.Empty), null));
		}

		[Test]
		public void IsIncomeWithZeroPriceFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddIncomeUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidPriceException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, 0, _incomeCategory, null));
		}

		[Test]
		public void IsIncomeWithUnknownAssetFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddIncomeUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<AssetNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, 1, _incomeCategory, new(string.Empty)));
		}

		[Test]
		public async Task IsExpenseAdded() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddExpenseUseCase(stateManager, new GuidIdGenerator());

			await addUseCase.Handle(_date, _userId, _brokerId, _accountId, 100, _expenseCategory, null);

			var state   = await stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _accountId);
			account.Balance.Should().Be(-100);
		}

		[Test]
		public void IsExpenseForUnknownBrokerFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddExpenseUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<BrokerNotFoundException>(() => addUseCase.Handle(
				_date, _userId, new(string.Empty), _accountId, 100, _expenseCategory, null));
		}

		[Test]
		public void IsExpenseForUnknownAccountFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddExpenseUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<AccountNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, new(string.Empty), 100, _expenseCategory, null));
		}

		[Test]
		public void IsExpenseWithEmptyCategoryFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddExpenseUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidCategoryException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, 100, new(string.Empty), null));
		}

		[Test]
		public void IsExpenseWithZeroPriceFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddExpenseUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<InvalidPriceException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, 0, _expenseCategory, null));
		}

		[Test]
		public void IsExpenseWithUnknownAssetFailedToAdd() {
			var stateManager = GetStateManager();
			var addUseCase   = new AddExpenseUseCase(stateManager, new GuidIdGenerator());

			Assert.ThrowsAsync<AssetNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, 1, _expenseCategory, new(string.Empty)));
		}

		[Test]
		public async Task IsIncomeOperationFound() {
			var stateManager = GetStateBuilder()
				.With(new AddIncomeModel(_date, _userId, _brokerId, _accountId, string.Empty, 100, _incomeCategory, _assetId))
				.Build();
			var readUseCase = new ReadAccountOperationsUseCase(stateManager);

			var operations = await readUseCase.Handle(_date, _date, _userId, _brokerId, _accountId);

			operations.Should().Contain(op =>
				(op.Date == _date) &&
				(op.Kind == OperationKind.Income) &&
				(op.Amount == 100) &&
				(op.Category == _incomeCategory) &&
				(op.Asset == _assetId));
		}

		[Test]
		public async Task IsExpenseOperationFound() {
			var stateManager = GetStateBuilder()
				.With(new AddExpenseModel(_date, _userId, _brokerId, _accountId, string.Empty, 50, _expenseCategory, _assetId))
				.Build();
			var readUseCase = new ReadAccountOperationsUseCase(stateManager);

			var operations = await readUseCase.Handle(_date, _date, _userId, _brokerId, _accountId);

			operations.Should().Contain(op =>
				(op.Date == _date) &&
				(op.Kind == OperationKind.Expense) &&
				(op.Amount == -50) &&
				(op.Category == _expenseCategory) &&
				(op.Asset == _assetId));
		}

		StateManagerBuilder GetStateBuilder() =>
			new StateManagerBuilder()
				.With(_userId).With(_brokerId).With(_currencyId).With(_accountId).With(_assetId, new(string.Empty), 1);

		StateManager GetStateManager() => GetStateBuilder().Build();
	}
}