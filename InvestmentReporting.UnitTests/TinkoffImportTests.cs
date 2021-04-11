using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.TinkoffBrokerReport;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;
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

			await AssertIncomeTransfers(stateManager);
		}

		[Test]
		public async Task IsIncomeTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_IncomeSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertIncomeTransfers(stateManager);
		}

		async Task AssertIncomeTransfers(StateManager stateManager) {
			var state      = await stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
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

			await AssertExpenseTransfers(stateManager);
		}

		[Test]
		public async Task IsExpenseTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_ExpenseSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertExpenseTransfers(stateManager);
		}

		async Task AssertExpenseTransfers(StateManager stateManager) {
			var state      = await stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
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
				new Trade(DateTimeOffset.Parse("2000-02-02T01:02:03+3"), "US0000000001", "AssetName1", 2, "USD", 200, 15),
				new Trade(DateTimeOffset.Parse("2000-02-02T01:02:03+3"), "RU0000000001", "AssetName2", 3, "RUB", 300, 18),
				new Trade(DateTimeOffset.Parse("2000-03-03T03:04:05+3"), "US0000000001", "AssetName1", -1, "USD", 101, 15),
				new Trade(DateTimeOffset.Parse("2000-03-03T03:04:05+3"), "RU0000000001", "AssetName2", -1, "RUB", 100, 18),
			};
			actualTrades.Should().Contain(expectedTrades);
		}

		[Test]
		public async Task IsAssetsImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_BuySellAssetSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertAssets(stateManager);
		}

		[Test]
		public async Task IsAssetsNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_BuySellAssetSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertAssets(stateManager);
		}

		async Task AssertAssets(StateManager stateManager) {
			var state  = await stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Isin == "US0000000001") && (a.Count == 1));
			broker.Inventory.Should().Contain(a =>
				(a.Isin == "RU0000000001") && (a.Count == 2));
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(-200 + 101);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(-300 + 100 - 15 - 18 - 15 - 18);
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

			await AssertDividendTransfers(stateManager);
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

			await AssertDividendTransfers(stateManager);
		}

		async Task AssertDividendTransfers(StateManager stateManager) {
			var state      = await stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(-100 + 0.3m);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void IsCouponTransfersRead() {
			var sample = LoadDocument("Tinkoff_BrokerMoneyMove_CouponSample.xlsx");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadCouponTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод погашение купона 0000-00-00000-0-0000 (Облигации ООО \"Организация\"  серии 0000-00) д.ф.22.03.21.(Удержан налог 16 руб.)", "RUB", 100),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public async Task IsCouponTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_CouponSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertCouponTransfers(stateManager);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public async Task IsCouponTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("Tinkoff_BrokerMoneyMove_CouponSample.xlsx");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertCouponTransfers(stateManager);
		}

		async Task AssertCouponTransfers(StateManager stateManager) {
			var state      = await stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(-100 - 10 + 100);
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
				.With(new CurrencyId("USD"))
				.With(_usdAccountId)
				.With(new CurrencyId("RUB"))
				.With(_rubAccountId)
				.Build();

		TinkoffImportUseCase GetUseCase(StateManager stateManager) {
			var loggerFactory     = new TestLoggerFactory();
			var transStateManager = new TransactionStateManager(loggerFactory, stateManager);
			var moneyMoveParser   = new BrokerMoneyMoveParser();
			var assetParser       = new AssetParser();
			var tradeParser       = new TradeParser();
			var idGenerator       = new GuidIdGenerator();
			var addIncomeUseCase  = new AddIncomeUseCase(stateManager, idGenerator);
			var addExpenseUseCase = new AddExpenseUseCase(stateManager, idGenerator);
			return new TinkoffImportUseCase(
				transStateManager, moneyMoveParser, assetParser, tradeParser,
				addIncomeUseCase,
				addExpenseUseCase,
				new BuyAssetUseCase(stateManager, idGenerator, addExpenseUseCase),
				new SellAssetUseCase(stateManager, addIncomeUseCase, addExpenseUseCase));
		}
	}
}