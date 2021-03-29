using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AssetOperationTests {
		readonly DateTimeOffset _date       = DateTimeOffset.MinValue;
		readonly UserId         _userId     = new("user");
		readonly BrokerId       _brokerId   = new("broker");
		readonly CurrencyId     _currencyId = new("currency");
		readonly AccountId      _accountId  = new("account");
		readonly AssetTicker    _ticker     = new("TCKR");
		readonly AssetCategory  _category   = new("stock");

		[Test]
		public async Task IsAssetBought() {
			var stateManager = GetStateManager();
			var buyUseCase   = new BuyAssetUseCase(stateManager, new GuidIdGenerator());

			await buyUseCase.Handle(_date, _userId, _brokerId, string.Empty, _category, _ticker, _currencyId, 0, 1);

			var state   = await stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Category == _category) &&
				(a.Ticker == _ticker) &&
				(a.Count == 1));
		}

		StateManagerBuilder GetStateBuilder() =>
			new StateManagerBuilder().With(_userId).With(_brokerId).With(_currencyId).With(_accountId);

		StateManager GetStateManager() =>
			new StateManagerBuilder().With(_userId).With(_brokerId).With(_currencyId).With(_accountId).Build();
	}
}