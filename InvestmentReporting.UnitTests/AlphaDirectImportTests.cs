using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Domain.UseCase.Exceptions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;
using InvestmentReporting.Import.UseCase;
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
			var sample       = LoadSample("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase      = GetUseCase(stateManager);

			Assert.ThrowsAsync<BrokerNotFoundException>(() => useCase.Handle(_date, _userId, _brokerId, sample));
		}

		[Test]
		public void IsFailedToImportToUnknownAccount() {
			var stateManager = new StateManagerBuilder()
				.With(_userId)
				.With(_brokerId)
				.Build();
			var sample  = LoadSample("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase = GetUseCase(stateManager);

			Assert.ThrowsAsync<AccountNotFoundException>(() => useCase.Handle(_date, _userId, _brokerId, sample));
		}

		[Test]
		public void IsIncomeTransfersRead() {
			var sample = LoadSample("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			var expectedTransfers = new[] {
				new IncomeTransfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод из BankName", "USD", 100),
				new IncomeTransfer(DateTimeOffset.Parse("2020-01-02T02:03:04+3"), "Перевод из BankName", "RUB", 200),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		[Test]
		public void IsIncomeTransfersSkipDividend() {
			var sample = LoadSample("AlphaDirect_BrokerMoneyMove_DividendSample.xml");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public void IsIncomeTransfersSkipExpenseTransfer() {
			var sample = LoadSample("AlphaDirect_BrokerMoneyMove_ExpenseSample.xml");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			actualTransfers.Should().BeEmpty();
		}

		[Test]
		public async Task IsIncomeTransfersImported() {
			var stateManager = GetStateManager();
			var sample       = LoadSample("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase      = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertIncomeTransfers(stateManager);
		}

		[Test]
		public async Task IsIncomeTransfersNotDuplicated() {
			var stateManager = GetStateManager();
			var sample       = LoadSample("AlphaDirect_BrokerMoneyMove_IncomeSample.xml");
			var useCase      = GetUseCase(stateManager);

			await useCase.Handle(_date, _userId, _brokerId, sample);
			await useCase.Handle(_date, _userId, _brokerId, sample);

			await AssertIncomeTransfers(stateManager);
		}

		async Task AssertIncomeTransfers(StateManager stateManager) {
			var state      = await stateManager.ReadState(DateTimeOffset.MaxValue, _userId);
			var broker     = state.Brokers.First(b => b.Id == _brokerId);
			var usdAccount = broker.Accounts.First(a => a.Id == _usdAccountId);
			usdAccount.Balance.Should().Be(100);
			var rubAccount = broker.Accounts.First(a => a.Id == _rubAccountId);
			rubAccount.Balance.Should().Be(200);
		}

		XmlDocument LoadSample(string name) {
			using var file = File.OpenRead(Path.Combine("Samples", name));
			var xml = new XmlDocument();
			xml.Load(file);
			return xml;
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

		ImportUseCase GetUseCase(StateManager stateManager) {
			var transStateManager = new TransactionStateManager(stateManager);
			var moneyMoveParser   = new BrokerMoneyMoveParser();
			var idGenerator       = new GuidIdGenerator();
			return new ImportUseCase(transStateManager, moneyMoveParser, new AddIncomeUseCase(stateManager, idGenerator));
		}
	}
}