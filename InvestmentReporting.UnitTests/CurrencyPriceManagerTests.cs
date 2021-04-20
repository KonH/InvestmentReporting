using System;
using System.Collections.Generic;
using FluentAssertions;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Market.Logic;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class CurrencyPriceManagerTests {
		readonly IReadOnlyCollection<CurrencyPriceModel> _models = new[] {
			new CurrencyPriceModel("USD", string.Empty, new List<CandleModel> {
				new(DateTimeOffset.MinValue, 100, 100, 100, 100)
			}),
			new CurrencyPriceModel("EUR", string.Empty, new List<CandleModel> {
				new(DateTimeOffset.MinValue, 200, 200, 200, 200)
			})
		};

		[Test]
		public void IsDirectPriceValid() {
			var manager = GetManager();

			var price = manager.GetPriceAt(new("RUB"), new("USD"), DateTimeOffset.MaxValue);
			price.Should().Be(1m / 100);
		}

		[Test]
		public void IsInversePriceValid() {
			var manager = GetManager();

			var price = manager.GetPriceAt(new("USD"), new("RUB"), DateTimeOffset.MaxValue);
			price.Should().Be(100);
		}

		[Test]
		public void IsTransferPriceFromUsdValid() {
			var manager = GetManager();

			var price = manager.GetPriceAt(new("USD"), new("EUR"), DateTimeOffset.MaxValue);
			price.Should().Be(0.5m);
		}

		[Test]
		public void IsTransferPriceFromEurValid() {
			var manager = GetManager();

			var price = manager.GetPriceAt(new("EUR"), new("USD"), DateTimeOffset.MaxValue);
			price.Should().Be(2m);
		}

		CurrencyPriceManager GetManager() {
			var repository = new Mock<ICurrencyPriceRepository>();
			repository
				.Setup(r => r.GetAll())
				.Returns(_models);
			return new CurrencyPriceManager(
				new TestLoggerFactory().CreateLogger<CurrencyPriceManager>(),
				new StateManagerBuilder().Build(),
				repository.Object);
		}
	}
}