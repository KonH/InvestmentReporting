using System;
using System.Collections.Generic;
using FluentAssertions;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class CurrencyIntervalCalculatorTests {
		readonly DateTimeOffset _startDate    = DateTimeOffset.UtcNow.AddYears(-3);
		readonly CurrencyCode   _currencyCode = new("USD");
		readonly AssetFIGI      _figi         = new("figi");

		[Test]
		public void IsIntervalWithoutPricesSplitIntoYears() {
			var calculator = GetCalculator();

			var intervals = calculator.TryCalculateRequiredIntervals(_figi);
			intervals.Should().HaveCount(3);
		}

		[Test]
		public void IsIntervalWithPriceIsShorter() {
			var prices = new [] {
				new CurrencyPriceModel(_currencyCode, _figi, new List<CandleModel> {
					new(_startDate.AddYears(2), 0, 0, 0, 0)
				})
			};
			var calculator = GetCalculator(prices);

			var intervals = calculator.TryCalculateRequiredIntervals(_figi);
			intervals.Should().HaveCount(1);
		}

		[Test]
		public void IsIntervalWithFreshPriceIsEmpty() {
			var prices = new [] {
				new CurrencyPriceModel(_currencyCode, _figi, new List<CandleModel> {
					new(_startDate.AddYears(3), 0, 0, 0, 0)
				})
			};
			var calculator = GetCalculator(prices);

			var intervals = calculator.TryCalculateRequiredIntervals(_figi);
			intervals.Should().BeEmpty();
		}

		CurrencyIntervalCalculator GetCalculator(params CurrencyPriceModel[] models) {
			var loggerFactory = new TestLoggerFactory();
			var stateManager  = new StateManagerBuilder()
				.With(_startDate)
				.With(_currencyCode)
				.With(new BrokerId(string.Empty))
				.Build();
			var repository    = new Mock<ICurrencyPriceRepository>();
			repository.Setup(r => r.GetAll())
				.Returns(models);
			var priceManager = new CurrencyPriceManager(
				loggerFactory.CreateLogger<CurrencyPriceManager>(), stateManager, repository.Object);
			return new(
				loggerFactory.CreateLogger<CurrencyIntervalCalculator>(),
				priceManager);
		}
	}
}