using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using InvestmentReporting.Import.Exceptions;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Import.AlphaDirect {
	public sealed class TransferParser {
		const string DetailsXPath =
			"Report[@Name=\"MyBroker\"]/Trades3/Report[@Name=\"4_Transfers\"]/Tablix1/Details_Collection/Details";

		readonly ILogger _logger;

		public TransferParser(ILogger<TransferParser> logger) {
			_logger = logger;
		}

		public IReadOnlyCollection<AssetTransfer> ReadAssetTransfers(XmlDocument report) {
			var detailsNodes = report.SelectNodes(DetailsXPath);
			if ( detailsNodes == null ) {
				throw new UnexpectedFormatException($"Failed to retrieve details via XPath '{DetailsXPath}'");
			}
			var result = new List<AssetTransfer>();
			_logger.LogTrace($"Found {detailsNodes.Count} transfer nodes");
			foreach ( XmlNode detailsNode in detailsNodes ) {
				ProcessDetails(detailsNode, result);
			}
			return result;
		}

		void ProcessDetails(XmlNode detailsNode, List<AssetTransfer> result) {
			if ( detailsNode.Attributes == null ) {
				throw new UnexpectedFormatException("Failed to retrieve details attributes");
			}
			var settlementDate = detailsNode.Attributes["settlement_date"]?.Value ?? string.Empty;
			var settlementTime = detailsNode.Attributes["settlement_time"]?.Value ?? string.Empty;
			var name           = detailsNode.Attributes["p_name"]?.Value ?? string.Empty;
			var qty            = detailsNode.Attributes["qty"]?.Value ?? string.Empty;
			_logger.LogTrace($"Raw values for transfer: {settlementDate}, {settlementTime}, {name}, {qty}");
			// We expect that it's Moscow time, but no timezone provided
			// and for backward-compatibility we should use fixed value
			var dateStr = $"{settlementDate} +3:00";
			if ( !DateTimeOffset.TryParseExact(dateStr, "yyyy-MM-ddTHH:mm:ss zzz", null, DateTimeStyles.None, out var dateComponent) ) {
				throw new UnexpectedFormatException($"Failed to parse date from '{settlementDate}'");
			}
			if ( !TimeSpan.TryParse(settlementTime, out var timeComponent) ) {
				throw new UnexpectedFormatException($"Failed to parse time from '{settlementTime}'");
			}
			var date = dateComponent.Add(timeComponent);
			if ( !int.TryParse(qty, out var count) ) {
				throw new UnexpectedFormatException($"Failed to parse count from '{qty}'");
			}
			result.Add(new(date, name, count));
		}
	}
}