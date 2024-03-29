using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using InvestmentReporting.Import.AlphaDirect;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AlphaDirectImportTests {
		readonly DateTimeOffset _date         = DateTimeOffset.MaxValue;
		readonly UserId         _userId       = new("user");
		readonly BrokerId       _brokerId     = new("broker");
		readonly AccountId      _usdAccountId = new("usd account");
		readonly AccountId      _rubAccountId = new("rub account");

		[Test]
		public void IsFailedToImportToUnknownBroker() {
			var stateManager = new StateManagerBuilder().Build();
			using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<BrokerNotFoundException>(() => useCase.Handle(_date, _userId, _brokerId, sample));
		}

		[Test]
		public void IsFailedToImportToUnknownAccount() {
			var stateManager = new StateManagerBuilder()
				.With(_userId)
				.With(_brokerId)
				.Build();
			using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<AccountNotFoundException>(() => useCase.Handle(_date, _userId, _brokerId, sample));
		}

		[Test]
		public void IsIncomeTransfersRead() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод из BankName", "USD", 100),
				new Transfer(DateTimeOffset.Parse("2020-01-02T02:03:04+3"), "Перевод из BankName", "RUB", 200),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public void IsIncomeTransfersSkipDividend() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_DividendSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public void IsIncomeTransfersSkipExpenseTransfer() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_ExpenseSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public async Task IsIncomeTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertIncomeTransfers(stateManager);
		}

		[Test]
		public async Task IsIncomeTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertIncomeTransfers(stateManager);
		}

		void AssertIncomeTransfers(StateManager stateManager) {
			var state      = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(100);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(200);
		}

		[Test]
		public void IsExpenseTransfersRead() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_ExpenseSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadExpenseTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод Списание по поручению клиента.", "USD", -100),
				new Transfer(DateTimeOffset.Parse("2020-01-02T02:03:04+3"), "Перевод Списание по поручению клиента.", "RUB", -200),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public void IsExpenseTransfersSkipDividend() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_DividendSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadExpenseTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public void IsExpenseTransfersSkipIncomeTransfer() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadExpenseTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public async Task IsExpenseTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_ExpenseSample.xml");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertExpenseTransfers(stateManager);
		}

		[Test]
		public async Task IsExpenseTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_ExpenseSample.xml");
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
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_BuySellAssetSample.xml");
			var parser = new TradeParser(new TestLoggerFactory().CreateLogger<TradeParser>());

			var actualTrades = parser.ReadTrades(sample);

			var expectedTrades = new[] {
				new Trade(DateTimeOffset.Parse("2000-02-02T01:02:03+3"), "US0000000001", "AssetName1, а.о.", 2, "USD", 200, 20),
				new Trade(DateTimeOffset.Parse("2000-02-02T01:02:03+3"), "RU0000000001", "AssetName2, о.к.б.", 3, "RUB", 300, 30),
				new Trade(DateTimeOffset.Parse("2000-03-03T03:04:05+3"), "US0000000001", "AssetName1, а.о.", -1, "USD", 101, 10.1m),
				new Trade(DateTimeOffset.Parse("2000-03-03T03:04:05+3"), "RU0000000001", "AssetName2, о.к.б.", -1, "RUB", 100, 10),
			};
			actualTrades.Should().Contain(expectedTrades);
		}

		[Test]
		public async Task IsAssetsImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_BuySellAssetSample.xml");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertAssets(stateManager);
		}

		[Test]
		public async Task IsAssetsNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_BuySellAssetSample.xml");
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
			rubAccount.Balance.Should().Be(-300 + 100 - 20 - 30 - 10.1m - 10);
		}

		[Test]
		public void IsDividendTransfersRead() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_DividendSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadDividendTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод {VO00001} Cash Dividend US0000000001 (NAME - XXX YYY) TAX 0.1 USD", "USD", 0.3m),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public async Task IsDividendTransfersImported() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_DividendSample.xml");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertDividendTransfers(stateManager);
		}

		[Test]
		public async Task IsDividendTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_DividendSample.xml");
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
		public void IsCouponTransfersReadSimple() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_CouponSample.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadCouponTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод погашение купона 0000-00-00000-0-0000 (Облигации ООО \"Организация\"  серии 0000-00) д.ф.22.03.21.(Удержан налог 16 руб.)", "RUB", 100),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public void IsCouponTransfersReadOfz() {
			var sample = LoadXml("AlphaDirect_BrokerMoneyMove_CouponSample2.xml");
			var parser = GetBrokerMoneyMoveParser();

			var actualTransfers = parser.ReadCouponTransfers(sample);

			var expectedTransfers = new[] {
				new Transfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод погашение купона 000000000 (Минфин России выпуск 00001) д.ф.22.03.21.(Удержан налог 16 руб.)", "RUB", 100),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public async Task IsCouponTransfersImportedOfz() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_CouponSample2.xml");
			var useCase = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertCouponTransfers(stateManager);
		}

		[Test]
		public async Task IsCouponTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			await using var sample = LoadStream("AlphaDirect_BrokerMoneyMove_CouponSample.xml");
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
			rubAccount.Balance.Should().Be(-100 - 10 + 100);
		}

		[Test]
		public async Task IsBondLifecycleImported() {
			var             stateManager = GetStateManager();
			await using var sample       = LoadStream("AlphaDirect_BrokerMoneyMove_BondLifecycleSample.xml");
			var             useCase      = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertBondLifecycle(stateManager);
		}

		[Test]
		public async Task IsBondLifecycleNotDuplicated() {
			var             stateManager = GetStateManager();
			await using var sample       = LoadStream("AlphaDirect_BrokerMoneyMove_BondLifecycleSample.xml");
			var             useCase      = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			sample.Position = 0;
			await useCase.Handle(_date, _userId, _brokerId, sample);

			AssertBondLifecycle(stateManager);
		}

		void AssertBondLifecycle(StateManager stateManager) {
			var state      = stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(-(1000 + 10 + 3) + (30 + 1000));
			broker.Inventory.Sum(a => a.Count).Should().Be(0);
		}

		Stream LoadStream(string name) => File.OpenRead(Path.Combine("Samples", name));

		XmlDocument LoadXml(string name) {
			using var file = LoadStream(name);
			var xml = new XmlDocument();
			xml.Load(file);
			return xml;
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

		BrokerMoneyMoveParser GetBrokerMoneyMoveParser() =>
			new(new TestLoggerFactory().CreateLogger<BrokerMoneyMoveParser>());

		AlphaDirectImportUseCase GetUseCase(StateManager stateManager) {
			var loggerFactory     = new TestLoggerFactory();
			var transStateManager = new TransactionStateManager(loggerFactory, stateManager);
			var moneyMoveParser   = new BrokerMoneyMoveParser(loggerFactory.CreateLogger<BrokerMoneyMoveParser>());
			var tradeParser       = new TradeParser(loggerFactory.CreateLogger<TradeParser>());
			var couponParser      = new CouponParser(loggerFactory.CreateLogger<CouponParser>());
			var transferParser    = new TransferParser(loggerFactory.CreateLogger<TransferParser>());
			var idGenerator       = new GuidIdGenerator();
			var addIncomeUseCase  = new AddIncomeUseCase(stateManager, idGenerator);
			var addExpenseUseCase = new AddExpenseUseCase(stateManager, idGenerator);
			return new AlphaDirectImportUseCase(
				transStateManager, moneyMoveParser, tradeParser, couponParser, transferParser,
				addIncomeUseCase,
				addExpenseUseCase,
				new BuyAssetUseCase(stateManager, idGenerator, addExpenseUseCase),
				new SellAssetUseCase(stateManager, addIncomeUseCase, addExpenseUseCase),
				new ReduceAssetUseCase(stateManager));
		}
	}
}