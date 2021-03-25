using System;
using System.IO;
using System.Xml;
using FluentAssertions;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Logic;
using NUnit.Framework;

namespace InvestmentReporting.UnitTests {
	public sealed class AlphaDirectImportTests {
		[Test]
		public void IsIncomeTransfersRead() {
			var sample = LoadSample("AlphaDirect_BrokerMoneyMove_Sample.xml");
			var parser = new BrokerMoneyMoveParser();

			var actualTransfers = parser.ReadIncomeTransfers(sample);

			var expectedTransfers = new[] {
				new IncomeTransfer(DateTimeOffset.Parse("2020-01-01T01:02:03+3"), "Перевод из BankName", "USD", 100),
				new IncomeTransfer(DateTimeOffset.Parse("2020-01-02T02:03:04+3"), "Перевод из BankName", "RUB", 200),
			};
			actualTransfers.Should().Contain(expectedTransfers);
		}

		XmlDocument LoadSample(string name) {
			using var file = File.OpenRead(Path.Combine("Samples", name));
			var xml = new XmlDocument();
			xml.Load(file);
			return xml;
		}
	}
}