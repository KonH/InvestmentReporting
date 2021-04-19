using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using InvestmentReporting.Data.InMemory.Repository;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.State.UseCase.Exceptions;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AssetOperationTests {
		readonly DateTimeOffset _date         = DateTimeOffset.MinValue;
		readonly UserId         _userId       = new("user");
		readonly BrokerId       _brokerId     = new("broker");
		readonly CurrencyCode   _currencyCode = new("USD");
		readonly AccountId      _payAccountId = new("payAccount");
		readonly AccountId      _feeAccountId = new("feeAccount");
		readonly AssetISIN      _isin         = new("TCKR");
		readonly AssetId        _assetId      = new("asset");

		[Test]
		public async Task IsAssetBought() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			await buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, 1, string.Empty, 1);

			var state  = stateManager.ReadState(_date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Isin == _isin) &&
				(a.Count == 1));
		}

		[Test]
		public async Task IsBuyOperationMarkedWithAsset() {
			var stateManager = GetStateManagerWithAsset();
			var buyUseCase   = GetBuyUseCase(stateManager);

			await buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, 1, string.Empty, 1);

			var operations = stateManager.ReadAccountOperations(_date, _userId, _brokerId, _payAccountId);
			operations.Should().NotBeEmpty();
			operations.Should().Contain(o => o.Asset == _assetId);
		}

		[Test]
		public async Task IsFeeOperationMarkedWithAsset() {
			var stateManager = GetStateManagerWithAsset();
			var buyUseCase   = GetBuyUseCase(stateManager);

			await buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, 1, string.Empty, 1);

			var operations = stateManager.ReadAccountOperations(_date, _userId, _brokerId, _feeAccountId);
			operations.Should().NotBeEmpty();
			operations.Should().Contain(o => o.Asset == _assetId);
		}

		[Test]
		public async Task IsAssetAccumulatedOnBought() {
			var stateManager = GetStateManagerWithAsset();
			var buyUseCase   = GetBuyUseCase(stateManager);

			await buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, 1, string.Empty, 1);

			var state  = stateManager.ReadState(_date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Isin == _isin) &&
				(a.Count == 3));
		}

		[Test]
		public async Task IsBalanceChangedAfterBuyByPrice() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);
			var price        = 100;

			await buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, price, 1, string.Empty, 1);

			var state   = stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _payAccountId);
			account.Balance.Should().Be(-price);
		}

		[Test]
		public async Task IsBalanceChangedAfterBuyByFee() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);
			var fee          = 100;

			await buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, fee, string.Empty, 1);

			var state   = stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _feeAccountId);
			account.Balance.Should().Be(-fee);
		}

		[Test]
		public void IsZeroPriceAssetBought() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.DoesNotThrowAsync(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 0, 1, string.Empty, 1));
		}

		[Test]
		public void IsZeroFeeAssetBought() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.DoesNotThrowAsync(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, 0, string.Empty, 1));
		}

		[Test]
		public void IsAssetFailedToBuyToUnknownBroker() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidBrokerException>(() => buyUseCase.Handle(
				_date, _userId, new(string.Empty), _payAccountId, _feeAccountId, _isin, 1, 1, string.Empty, 1));
		}

		[Test]
		public void IsAssetFailedToBuyToUnknownPayAccount() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAccountException>(() => buyUseCase.Handle(
				_date, _userId, _brokerId, new(string.Empty), _feeAccountId, _isin, 1, 1, string.Empty, 1));
		}

		[Test]
		public void IsAssetFailedToBuyToUnknownFeeAccount() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAccountException>(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, new(string.Empty), _isin, 1, 1, string.Empty, 1));
		}

		[Test]
		public void IsAssetFailedToBuyWithEmptyISIN() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAssetException>(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, new(string.Empty), 1, 1, string.Empty, 1));
		}

		[Test]
		public void IsAssetFailedToBuyWithInvalidCount() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidCountException>(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, 1, string.Empty, 0));
		}

		[Test]
		public void IsAssetFailedToBuyWithInvalidPrice() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidPriceException>(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin,-1, 1, string.Empty, 1));
		}

		[Test]
		public void IsAssetFailedToBuyWithInvalidFee() {
			var stateManager = GetStateManager();
			var buyUseCase   = GetBuyUseCase(stateManager);

			Assert.ThrowsAsync<InvalidPriceException>(() => buyUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _isin, 1, -1, string.Empty, 1));
		}

		[Test]
		public async Task IsAssetSoldCompletely() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			await sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 1, 2);

			var state  = stateManager.ReadState(_date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Isin == _isin) &&
				(a.Count == 0));
		}

		[Test]
		public async Task IsAssetSoldPartially() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			await sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 1, 1);

			var state  = stateManager.ReadState(_date, _userId);
			var broker = state.Brokers.First(b => b.Id == _brokerId);
			broker.Inventory.Should().Contain(a =>
				(a.Isin == _isin) &&
				(a.Count == 1));
		}

		[Test]
		public async Task IsSellPriceOperationMarkedWithAsset() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			await sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 1, 1);

			var operations = stateManager.ReadAccountOperations(_date, _userId, _brokerId, _payAccountId);
			operations.Should().NotBeEmpty();
			operations.Should().Contain(o => o.Asset == _assetId);
		}

		[Test]
		public async Task IsSellFeeOperationMarkedWithAsset() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			await sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 1, 1);

			var operations = stateManager.ReadAccountOperations(_date, _userId, _brokerId, _feeAccountId);
			operations.Should().NotBeEmpty();
			operations.Should().Contain(o => o.Asset == _assetId);
		}

		[Test]
		public async Task IsBalanceChangedAfterSellByPrice() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);
			var price        = 100;

			await sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, price, 1, 1);

			var state   = stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _payAccountId);
			account.Balance.Should().Be(price);
		}

		[Test]
		public async Task IsBalanceChangedAfterSellByFee() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);
			var fee          = 100;

			await sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, fee, 1);

			var state   = stateManager.ReadState(_date, _userId);
			var broker  = state.Brokers.First(b => b.Id == _brokerId);
			var account = broker.Accounts.First(a => a.Id == _feeAccountId);
			account.Balance.Should().Be(-fee);
		}

		[Test]
		public void IsZeroPriceAssetSold() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.DoesNotThrowAsync(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 0, 1, 1));
		}

		[Test]
		public void IsZeroFeeAssetSold() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.DoesNotThrowAsync(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 0, 1));
		}

		[Test]
		public void IsAssetFailedToSellToUnknownBroker() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidBrokerException>(() => sellUseCase.Handle(
				_date, _userId, new(string.Empty), _payAccountId, _feeAccountId, _assetId, 1, 1, 1));
		}

		[Test]
		public void IsAssetFailedToSellToUnknownPayAccount() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAccountException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, new(string.Empty), _feeAccountId, _assetId, 1, 1, 1));
		}

		[Test]
		public void IsAssetFailedToSellToUnknownFeeAccount() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAccountException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, new(string.Empty), _assetId, 1, 1, 1));
		}

		[Test]
		public void IsAssetFailedToSellWithInvalidId() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidAssetException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, new(string.Empty), 1, 1, 1));
		}

		[Test]
		public void IsAssetFailedToSellWithInvalidCount() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidCountException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 1, 0));
		}

		[Test]
		public void IsAssetFailedToSellWithMoreThanAvailableCount() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidCountException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, 1, 3));
		}

		[Test]
		public void IsAssetFailedToSellWithInvalidPrice() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidPriceException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, -1, 1, 1));
		}

		[Test]
		public void IsAssetFailedToSellWithInvalidFee() {
			var stateManager = GetStateManagerWithAsset();
			var sellUseCase  = GetSellUseCase(stateManager);

			Assert.ThrowsAsync<InvalidPriceException>(() => sellUseCase.Handle(
				_date, _userId, _brokerId, _payAccountId, _feeAccountId, _assetId, 1, -1, 1));
		}

		BuyAssetUseCase GetBuyUseCase(StateManager stateManager) {
			var idGenerator = new GuidIdGenerator();
			return new BuyAssetUseCase(stateManager, idGenerator, new AddExpenseUseCase(stateManager, idGenerator));
		}

		SellAssetUseCase GetSellUseCase(StateManager stateManager) {
			var idGenerator = new GuidIdGenerator();
			return new SellAssetUseCase(
				stateManager,
				new AddIncomeUseCase(stateManager, idGenerator),
				new AddExpenseUseCase(stateManager, idGenerator));
		}

		StateManagerBuilder GetStateBuilder() =>
			new StateManagerBuilder().With(_userId).With(_brokerId).With(_currencyCode)
				.With(_payAccountId).With(_feeAccountId);

		StateManager GetStateManager() => GetStateBuilder().Build();

		StateManager GetStateManagerWithAsset() =>
			GetStateBuilder().With(_assetId, _isin, 2).Build();
	}
}