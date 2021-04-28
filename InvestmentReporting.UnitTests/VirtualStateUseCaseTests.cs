using System;
using System.Collections.Generic;
using FluentAssertions;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.Market.UseCase;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class VirtualStateUseCaseTests {
		readonly DateTimeOffset _candleDate   = DateTimeOffset.MinValue.AddYears(1);
		readonly DateTimeOffset _periodDate   = DateTimeOffset.MinValue.AddYears(2);
		readonly UserId         _userId       = new("user");
		readonly BrokerId       _brokerId     = new("broker");
		readonly CurrencyCode   _currencyCode = new("USD");
		readonly AccountId      _accountId    = new("account");
		readonly AssetId        _assetId      = new("asset");
		readonly AssetISIN      _assetIsin    = new("isin");
		readonly decimal        _virtualPrice = 10;

		[Test]
		public void IsStateContainsBalance() {
			var useCase = GetUseCase(GetStateManager());

			var state = useCase.Handle(_periodDate, _userId, VirtualPeriod.CalendarYear);

			state.Balances.Should().Contain(b =>
				(b.Currency == _currencyCode) &&
				(b.VirtualSum == _virtualPrice));
		}

		[Test]
		public void IsStateContainsSummary() {
			var useCase = GetUseCase(GetStateManager());

			var state = useCase.Handle(_periodDate, _userId, VirtualPeriod.CalendarYear);

			state.Summary.Should().Contain(p =>
				(p.Key == _currencyCode) &&
				(p.Value.VirtualSum == _virtualPrice));
		}

		StateManager GetStateManager() =>
			new StateManagerBuilder()
				.With(_userId)
				.With(_brokerId)
				.With(_currencyCode)
				.With(_accountId)
				.With(_assetId, _assetIsin, 1)
				.Build();

		ReadVirtualStateUseCase GetUseCase(IStateManager stateManager) {
			var loggerFactory           = new TestLoggerFactory();
			var assetMetadataRepository = new Mock<IAssetMetadataRepository>();
			assetMetadataRepository.Setup(m => m.GetAll())
				.Returns(new[] { new AssetMetadataModel(_assetIsin, string.Empty, string.Empty, string.Empty) });
			var metadataManager = new MetadataManager(
				loggerFactory.CreateLogger<MetadataManager>(), assetMetadataRepository.Object, stateManager);
			var currencyConfig       = new CurrencyConfiguration();
			var assetPriceRepository = new Mock<IAssetPriceRepository>();
			assetPriceRepository.Setup(m => m.GetAll())
				.Returns(new[] { new AssetPriceModel(_assetIsin, string.Empty, new List<CandleModel> {
					new(_candleDate, 0, 10, 0, 0)
				})});
			var priceManager = new AssetPriceManager(
				loggerFactory.CreateLogger<AssetPriceManager>(), assetPriceRepository.Object, stateManager);
			var currencyPriceRepository = new Mock<ICurrencyPriceRepository>();
			currencyPriceRepository.Setup(m => m.GetAll())
				.Returns(new[] {
					new CurrencyPriceModel(_currencyCode, string.Empty, new List<CandleModel> {
						new(_candleDate, 0, 1, 0, 0)
					})
				});
			var currencyPriceManager = new CurrencyPriceManager(
				loggerFactory.CreateLogger<CurrencyPriceManager>(), stateManager, currencyPriceRepository.Object);
			var exchangeManager = new ExchangeManager(
				loggerFactory.CreateLogger<ExchangeManager>(), currencyPriceManager);
			return new(
				loggerFactory.CreateLogger<ReadVirtualStateUseCase>(),
				stateManager, metadataManager, currencyConfig, priceManager, exchangeManager);
		}
	}
}