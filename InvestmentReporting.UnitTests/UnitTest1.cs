using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.Model;
using InvestmentReporting.Domain.Repository;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public class Tests {
		readonly DateTimeOffset  _date            = DateTimeOffset.MinValue;
		readonly UserId          _userId          = new("user");
		readonly BrokerId        _brokerId        = new("broker");
		readonly CurrencyId      _currencyId      = new("currency");
		readonly AccountId       _accountId       = new("account");
		readonly IncomeCategory  _incomeCategory  = new("income_category");
		readonly ExpenseCategory _expenseCategory = new("expense_category");

		[Test]
		public async Task IsStateCreated() {
			var stateManager = GetStateManager().Build();
			var readUseCase  = new ReadStateUseCase(stateManager);

			var state = await readUseCase.Handle(_date, _userId);

			state.Should().NotBeNull();
		}

		[Test]
		public void IsStateFailedToCreateForUnknownUser() {
			var user         = new UserId(string.Empty);
			var stateManager = GetStateManager().Build();
			var readUseCase  = new ReadStateUseCase(stateManager);

			Assert.ThrowsAsync<InvalidUserException>(() => readUseCase.Handle(_date, user));
		}

		[Test]
		public async Task IsBrokerAdded() {
			var brokerName    = "BrokerName";
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateBrokerUseCase(stateManager, new IdGenerator());

			await createUseCase.Handle(_date, _userId, brokerName);

			var state = await ReadState(stateManager, _date, _userId);
			state.Brokers.Should().NotBeEmpty();
			state.Brokers.Should().Contain(b => b.DisplayName == brokerName);
		}

		[Test]
		public void IsBrokerWithEmptyNameFailedToAdd() {
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateBrokerUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidBrokerException>(() => createUseCase.Handle(_date, _userId, string.Empty));
		}

		[Test]
		public async Task IsBrokerWithDuplicateNameFailedToAdd() {
			var brokerName    = "BrokerName";
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateBrokerUseCase(stateManager, new IdGenerator());
			await createUseCase.Handle(_date, _userId, brokerName);

			Assert.ThrowsAsync<DuplicateBrokerException>(() => createUseCase.Handle(_date, _userId, brokerName));
		}

		[Test]
		public async Task IsBrokerNotAddedInPast() {
			var brokerName    = "BrokerName";
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateBrokerUseCase(stateManager, new IdGenerator());

			await createUseCase.Handle(_date.AddSeconds(1), _userId, brokerName);

			var state = await ReadState(stateManager, _date, _userId);
			state.Brokers.Should().BeEmpty();
		}

		[Test]
		public async Task IsCurrencyAdded() {
			var code          = new CurrencyCode("USD");
			var format        = new CurrencyFormat("${0}");
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new IdGenerator());

			await createUseCase.Handle(_date, _userId, code, format);

			var state = await ReadState(stateManager, _date, _userId);
			state.Currencies.Should().NotBeEmpty();
			state.Currencies.Should().Contain(c => (c.Code == code) && (c.Format == format));
		}

		[Test]
		public void IsCurrencyWithEmptyCodeFailedToAdd() {
			var code          = new CurrencyCode(string.Empty);
			var format        = new CurrencyFormat("${0}");
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidCurrencyException>(() => createUseCase.Handle(_date, _userId, code, format));
		}

		[Test]
		public void IsCurrencyWithInvalidFormatFailedToAdd() {
			var code          = new CurrencyCode("USD");
			var format        = new CurrencyFormat("$");
			var stateManager  = GetStateManager().Build();
			var createUseCase = new CreateCurrencyUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidCurrencyException>(() => createUseCase.Handle(_date, _userId, code, format));
		}

		[Test]
		public async Task IsAccountAdded() {
			var accountName   = "Account";
			var stateManager  = GetStateManager().With(_userId).With(_brokerId).With(_currencyId).Build();
			var createUseCase = new CreateAccountUseCase(stateManager, new IdGenerator());

			await createUseCase.Handle(_date, _userId, _brokerId, _currencyId, accountName);

			var state  = await ReadState(stateManager, _date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Accounts.Should().NotBeEmpty();
			broker.Accounts.Should().Contain(a => (a.Currency == _currencyId) && (a.DisplayName == accountName));
		}

		[Test]
		public void IsAccountWithEmptyNameFailedToAdd() {
			var stateManager  = GetStateManager().With(_userId).With(_brokerId).With(_currencyId).Build();
			var createUseCase = new CreateAccountUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidAccountException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyId, string.Empty));
		}

		[Test]
		public void IsAccountForUnknownBrokerFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager().With(_userId).With(_currencyId).Build();
			var createUseCase = new CreateAccountUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<BrokerNotFoundException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyId, accountName));
		}

		[Test]
		public void IsAccountWithUnknownCurrencyFailedToAdd() {
			var accountName   = "Account";
			var stateManager  = GetStateManager().With(_userId).With(_brokerId).Build();
			var createUseCase = new CreateAccountUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<CurrencyNotFoundException>(() => createUseCase.Handle(_date, _userId, _brokerId, _currencyId, accountName));
		}

		[Test]
		public async Task IsIncomeAdded() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			await addUseCase.Handle(_date, _userId, _brokerId, _accountId, _currencyId, 100, exchangeRate: 1, _incomeCategory);

			var state   = await ReadState(stateManager, _date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _accountId);
			account.Balance.Should().Be(100);
		}

		[Test]
		public void IsIncomeForUnknownBrokerFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<BrokerNotFoundException>(() => addUseCase.Handle(
				_date, _userId, new(string.Empty), _accountId, _currencyId, 100, exchangeRate: 1, _incomeCategory));
		}

		[Test]
		public void IsIncomeForUnknownAccountFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<AccountNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, new(string.Empty), _currencyId, 100, exchangeRate: 1, _incomeCategory));
		}

		[Test]
		public void IsIncomeWithUnknownCurrencyFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<CurrencyNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, new(string.Empty), 100, exchangeRate: 1, _incomeCategory));
		}

		[Test]
		public void IsIncomeWithEmptyCategoryFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidCategoryException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, _currencyId, 100, exchangeRate: 1, new(string.Empty)));
		}

		[Test]
		public void IsIncomeWithZeroPriceFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidPriceException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, _currencyId, 0, exchangeRate: 1, _incomeCategory));
		}

		[Test]
		public void IsIncomeWithZeroExchangeRateFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddIncomeUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidPriceException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, _currencyId, 100, exchangeRate: 0, _incomeCategory));
		}

		[Test]
		public async Task IsExpenseAdded() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			await addUseCase.Handle(_date, _userId, _brokerId, _accountId, _currencyId, 100, exchangeRate: 1, _expenseCategory);

			var state   = await ReadState(stateManager, _date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _accountId);
			account.Balance.Should().Be(-100);
		}

		[Test]
		public void IsExpenseForUnknownBrokerFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<BrokerNotFoundException>(() => addUseCase.Handle(
				_date, _userId, new(string.Empty), _accountId, _currencyId, 100, exchangeRate: 1, _expenseCategory));
		}

		[Test]
		public void IsExpenseForUnknownAccountFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<AccountNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, new(string.Empty), _currencyId, 100, exchangeRate: 1, _expenseCategory));
		}

		[Test]
		public void IsExpenseWithUnknownCurrencyFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<CurrencyNotFoundException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, new(string.Empty), 100, exchangeRate: 1, _expenseCategory));
		}

		[Test]
		public void IsExpenseWithEmptyCategoryFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidCategoryException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, _currencyId, 100, exchangeRate: 1, new(string.Empty)));
		}

		[Test]
		public void IsExpenseWithZeroPriceFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidPriceException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, _currencyId, 0, exchangeRate: 1, _expenseCategory));
		}

		[Test]
		public void IsExpenseWithZeroExchangeRateFailedToAdd() {
			var stateManager = GetStateManagerForOperations();
			var addUseCase   = new AddExpenseUseCase(stateManager, new IdGenerator());

			Assert.ThrowsAsync<InvalidPriceException>(() => addUseCase.Handle(
				_date, _userId, _brokerId, _accountId, _currencyId, 100, exchangeRate: 0, _expenseCategory));
		}

		sealed class StateManagerBuilder {
			readonly List<ICommandModel> _commands = new();

			DateTimeOffset _date       = DateTimeOffset.MinValue;
			string         _userId     = string.Empty;
			string         _brokerId   = string.Empty;
			string         _currencyId = string.Empty;

			public StateManagerBuilder With(DateTimeOffset date) {
				_date = date;
				return this;
			}

			public StateManagerBuilder With(UserId user) {
				_userId = user.ToString();
				return this;
			}

			public StateManagerBuilder With(BrokerId broker) {
				_brokerId = broker.ToString();
				_commands.Add(new CreateBrokerModel(_date, _userId, _brokerId, string.Empty));
				return this;
			}

			public StateManagerBuilder With(CurrencyId currency) {
				_currencyId = currency.ToString();
				_commands.Add(new CreateCurrencyModel(_date, _userId, _currencyId, string.Empty, string.Empty));
				return this;
			}

			public StateManagerBuilder With(AccountId account) {
				_commands.Add(new CreateAccountModel(_date, _userId, _brokerId, account.ToString(), _currencyId, string.Empty));
				return this;
			}

			public StateManager Build() =>
				new(new StateRepository(_commands));
		}

		StateManagerBuilder GetStateManager() => new();

		StateManager GetStateManagerForOperations() =>
			GetStateManager().With(_userId).With(_brokerId).With(_currencyId).With(_accountId).Build();

		async Task<ReadOnlyState> ReadState(StateManager stateManager, DateTimeOffset date, UserId user) =>
			await new ReadStateUseCase(stateManager).Handle(date, user);
	}
}