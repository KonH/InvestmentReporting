using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Import.AlphaDirect {
	public sealed class TradeParser {
		const string DetailsXPath =
			"Report[@Name=\"MyBroker\"]/Trades/Report[@Name=\"2_Trades\"]/Tablix2/Details_Collection/Details";

		readonly ILogger _logger;

		public TradeParser(ILogger<TradeParser> logger) {
			_logger = logger;
		}

		public IReadOnlyCollection<Trade> ReadTrades(XmlDocument report) {
			var detailsNodes = report.SelectNodes(DetailsXPath);
			if ( detailsNodes == null ) {
				throw new UnexpectedFormatException($"Failed to retrieve details via XPath '{DetailsXPath}'");
			}
			_logger.LogTrace($"Found {detailsNodes.Count} trade nodes");
			var result = new List<Trade>();
			foreach ( XmlNode detailsNode in detailsNodes ) {
				ProcessDetails(detailsNode, result);
			}
			return result;
		}

		void ProcessDetails(XmlNode detailsNode, List<Trade> result) {
			if ( detailsNode.Attributes == null ) {
				throw new UnexpectedFormatException("Failed to retrieve details attributes");
			}
			var settlementTime = detailsNode.Attributes["settlement_time"]?.Value ?? string.Empty;
			var isin           = detailsNode.Attributes["isin_reg"]?.Value ?? string.Empty;
			var name           = detailsNode.Attributes["p_name"]?.Value ?? string.Empty;
			var qty            = detailsNode.Attributes["qty"]?.Value ?? string.Empty;
			var sumTrade       = detailsNode.Attributes["summ_trade"]?.Value ?? string.Empty;
			var currency       = detailsNode.Attributes["curr_calc"]?.Value ?? string.Empty;
			var bankTax        = detailsNode.Attributes["bank_tax"]?.Value ?? string.Empty;
			_logger.LogTrace(
				$"Raw values for trade: {settlementTime}, {isin}, {name}, {qty}, {sumTrade}, {currency}, {bankTax}");
			var timeParts = settlementTime.Split('\r');
			if ( timeParts.Length < 1 ) {
				throw new UnexpectedFormatException($"Failed to parse date from '{settlementTime}'");
			}
			// We expect that it's Moscow time, but no timezone provided
			// and for backward-compatibility we should use fixed value
			var dateStr = $"{timeParts[0]} +3:00";
			if ( !DateTimeOffset.TryParseExact(dateStr, "dd.MM.yyyy H:mm:ss zzz", null, DateTimeStyles.None, out var date) ) {
				throw new UnexpectedFormatException($"Failed to parse date from '{dateStr}'");
			}
			if ( !int.TryParse(qty, out var count) ) {
				throw new UnexpectedFormatException($"Failed to parse count from '{qty}'");
			}
			if ( !decimal.TryParse(sumTrade, out var price) ) {
				throw new UnexpectedFormatException($"Failed to parse price from '{sumTrade}'");
			}
			if (!decimal.TryParse(bankTax, out var fee)) {
				throw new UnexpectedFormatException($"Failed to parse fee from '{bankTax}'");
			}
			result.Add(new(date, isin, name, count, currency, price, fee));
		}
	}
}