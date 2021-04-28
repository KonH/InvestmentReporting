using System;
using System.Collections.Generic;
using FluentAssertions;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AssetIntervalCalculatorTests {
		readonly DateTimeOffset _buyDate = DateTimeOffset.UtcNow.AddYears(-3);
		readonly AssetISIN      _isin    = new("isin");

		[Test]
		public void IsIntervalWithoutPricesSplitIntoYears() {
			var calculator = GetCalculator();

			var intervals = calculator.TryCalculateRequiredIntervals(_isin);
			intervals.Should().HaveCount(3);
		}

		[Test]
		public void IsIntervalWithPriceIsShorter() {
			var prices = new [] {
				new AssetPriceModel(_isin, string.Empty, new List<CandleModel> {
					new(_buyDate.AddYears(2), 0, 0, 0, 0)
				})
			};
			var calculator = GetCalculator(prices);

			var intervals = calculator.TryCalculateRequiredIntervals(_isin);
			intervals.Should().HaveCount(1);
		}

		[Test]
		public void IsIntervalWithFreshPriceIsEmpty() {
			var prices = new [] {
				new AssetPriceModel(_isin, string.Empty, new List<CandleModel> {
					new(_buyDate.AddYears(3), 0, 0, 0, 0)
				})
			};
			var calculator = GetCalculator(prices);

			var intervals = calculator.TryCalculateRequiredIntervals(_isin);
			intervals.Should().BeEmpty();
		}

		AssetIntervalCalculator GetCalculator(params AssetPriceModel[] prices) {
			var loggerFactory = new TestLoggerFactory();
			var stateManager  = new StateManagerBuilder()
				.With(_buyDate)
				.With(new("asset"), _isin, 1)
				.Build();
			var repository    = new Mock<IAssetPriceRepository>();
			repository.Setup(r => r.GetAll())
				.Returns(prices);
			var priceManager = new AssetPriceManager(
				loggerFactory.CreateLogger<AssetPriceManager>(), repository.Object, stateManager);
			return new(
				loggerFactory.CreateLogger<AssetIntervalCalculator>(),
				priceManager);
		}
	}
}