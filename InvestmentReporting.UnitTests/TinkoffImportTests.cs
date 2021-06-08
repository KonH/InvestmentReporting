using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using InvestmentReporting.Import.Tinkoff;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class TinkoffImportTests {
		readonly DateTimeOffset _date         = DateTimeOffset.MaxValue;
		readonly UserId         _userId       = new("user");
		readonly BrokerId       _brokerId     = new("broker");
		readonly AccountId      _usdAccountId = new("usd account");
		readonly AccountId      _rubAccountId = new("rub account");

		[Test]
		public void IsFailedToImportToUnknownBroker() {
			var stateManager = new StateManagerBuilder().Build();
			using var sample = LoadStream("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var useCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<BrokerNotFoundException>(() => useCase.Handle(_date, _userId, _brokerId, sample));
		}

		[Test]
		public void IsFailedToImportToUnknownAccount() {
			var stateManager = new StateManagerBuilder()
				.With(_userId)
				.With(_brokerId)
				.Build();
			using var sample = LoadStream("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var useCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<AccountNotFoundException>(() => useCase.Handle(_date, _userId, _brokerId, sample));
		}

		[Test]
		public void IsIncomeTransfersRead() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2000-01-01T00:00:01+3"), "Пополнение счета", "RUB", 100),
				new Transfer(DateTimeOffset.Parse("2000-01-02T00:00:01+3"), "Пополнение счета", "USD", 200),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void IsIncomeTransfersSkipDividend() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_DividendSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public void IsIncomeTransfersSkipExpenseTransfer() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_ExpenseSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public async Task IsIncomeTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertIncomeTransfers(stateManager);
		}

		[Test]
		public async Task IsIncomeTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertIncomeTransfers(stateManager);
		}

		void AssertIncomeTransfers(StateManager stateManager) {
			var state      = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(100);
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(200);
		}

		[Test]
		public void IsExpenseTransfersRead() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_ExpenseSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadExpenseTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2000-01-01T01:02:03+3"), "Вывод средств", "USD", -100),
				new Transfer(DateTimeOffset.Parse("2000-01-01T02:03:04+3"), "Вывод средств", "RUB", -200),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void IsExpenseTransfersSkipDividend() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_DividendSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadExpenseTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public void IsExpenseTransfersSkipIncomeTransfer() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadExpenseTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public async Task IsExpenseTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_ExpenseSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertExpenseTransfers(stateManager);
		}

		[Test]
		public async Task IsExpenseTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_ExpenseSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertExpenseTransfers(stateManager);
		}

		void AssertExpenseTransfers(StateManager stateManager) {
			var state      = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(-100);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(-200);
		}

		[Test]
		public void IsTradesRead() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_BuySellAssetSample.xlsx");
			var parser = new TradeParser();

			var assets       = new AssetParser().ReadAssets(sample);
			var actualTrades = parser.ReadTrades(sample, assets);

			var expectedTrades = new[] {
				new Trade(DateTimeOffset.Parse("2000-02-02T01:02:03+3"), "US0000000001", "AssetName1", 2, "USD", 200, 5),
				new Trade(DateTimeOffset.Parse("2000-02-02T01:02:03+3"), "RU0000000001", "AssetName2", 3, "RUB", 300, 6),
				new Trade(DateTimeOffset.Parse("2000-03-03T03:04:05+3"), "US0000000001", "AssetName1", -1, "USD", 101, 5),
				new Trade(DateTimeOffset.Parse("2000-03-03T03:04:05+3"), "RU0000000001", "AssetName2", -1, "RUB", 100, 6),
			};
			actualTrades.Should().Contain(expectedTrades);
		}

		[Test]
		public async Task IsAssetsImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_BuySellAssetSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertAssets(stateManager);
		}

		[Test]
		public async Task IsAssetsNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_BuySellAssetSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertAssets(stateManager);
		}

		void AssertAssets(StateManager stateManager) {
			var state  = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Isin == "US0000000001") && (a.Count == 1));
			broker.Inventory.Should().Contain(a =>
				(a.Isin == "RU0000000001") && (a.Count == 2));
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(-200 + 101);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(-300 + 100 - 5 - 6 - 5 - 6);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void IsDividendTransfersRead() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_DividendSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadDividendTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод {VO00001} Cash Dividend US0000000001 (NAME - XXX YYY) TAX 0.1 USD", "USD", 0.3m),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public async Task IsDividendTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_DividendSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertDividendTransfers(stateManager);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public async Task IsDividendTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_DividendSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertDividendTransfers(stateManager);
		}

		void AssertDividendTransfers(StateManager stateManager) {
			var state      = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(-100 + 0.3m);
		}

		[Test]
		public void IsCouponTransfersRead() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_CouponSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadCouponTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2000-01-02T16:00:00+3"), "Выплата купонов ОблигацияN/ 1 шт.", "RUB", 150),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public async Task IsCouponTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_CouponSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertCouponTransfers(stateManager);
		}

		[Test]
		public async Task IsCouponTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_CouponSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertCouponTransfers(stateManager);
		}

		void AssertCouponTransfers(StateManager stateManager) {
			var state      = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be((1010 + 150 + 1000) - (1000 + 50));
		}

		Stream LoadStream(string name) => File.OpenRead(Path.Combine("Samples", name));

		IXLWorkbook LoadDocument(string name) {
			using var file = LoadStream(name);
			return new XLWorkbook(file);
		}

		StateManager GetStateManager() =>
			new StateManagerBuilder()
				.With(_userId)
				.With(_brokerId)
				.With(new CurrencyCode("USD"))
				.With(_usdAccountId)
				.With(new CurrencyCode("RUB"))
				.With(_rubAccountId)
				.Build();

		TinkoffImportUseCase GetUseCase(StateManager stateManager) {
			var loggerFactory     = new TestLoggerFactory();
			var transStateManager = new TransactionStateManager(loggerFactory, stateManager);
			var moneyMoveParser   = new BrokerMoneyMoveParser();
			var assetParser       = new AssetParser();
			var tradeParser       = new TradeParser();
			var couponParser      = new CouponParser(loggerFactory.CreateLogger<CouponParser>());
			var idGenerator       = new GuidIdGenerator();
			var addIncomeUseCase  = new AddIncomeUseCase(stateManager, idGenerator);
			var addExpenseUseCase = new AddExpenseUseCase(stateManager, idGenerator);
			return new TinkoffImportUseCase(
				transStateManager, moneyMoveParser, assetParser, tradeParser, couponParser,
				addIncomeUseCase,
				addExpenseUseCase,
				new BuyAssetUseCase(stateManager, idGenerator, addExpenseUseCase),
				new SellAssetUseCase(stateManager, addIncomeUseCase, addExpenseUseCase));
		}
	}
}