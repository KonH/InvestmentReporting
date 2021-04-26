using System;
using System.Collections.Generic;
using System.Xml;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;

namespace InvestmentReporting.Import.AlphaDirect {
	public sealed class BrokerMoneyMoveParser {
		const string OperationsXpath =
			"Report[@Name=\"MyBroker\"]/*/Report[@Name=\"3_BrokerMoneyMove\"]/Tablix1/settlement_date_Collection/settlement_date/rn_Collection/rn";

		const string OperationTypesXPath = "oper_type[@oper_type=\"Перевод\"]";

		const string CommentXpath = "comment";

		const string PCodesXpath =
			"money_volume_begin1_Collection/money_volume_begin1/p_code_Collection/p_code/p_code";

		public IReadOnlyCollection<Transfer> ReadIncomeTransfers(XmlDocument report) =>
			ReadTransfers(
				report,
				comment => comment.StartsWith("из "));

		public IReadOnlyCollection<Transfer> ReadDividendTransfers(XmlDocument report) =>
			ReadTransfers(
				report,
				comment => comment.Contains("Cash Dividend"));

		public IReadOnlyCollection<Transfer> ReadCouponTransfers(XmlDocument report) =>
			ReadTransfers(
				report,
				comment => comment.StartsWith("погашение купона"));

		public IReadOnlyCollection<Transfer> ReadRedemptionTransfers(XmlDocument report) =>
			ReadTransfers(
				report,
				comment => comment.StartsWith("полное погашение номинала"));

		public IReadOnlyCollection<Transfer> ReadExpenseTransfers(XmlDocument report) =>
			ReadTransfers(
				report,
				comment => comment.StartsWith("Списание по поручению клиента"));

		IReadOnlyCollection<Transfer> ReadTransfers(
			XmlDocument report, Func<string, bool> commentFilter) {
			var operations = report.SelectNodes(OperationsXpath);
			if ( operations == null ) {
				throw new UnexpectedFormatException($"Failed to retrieve operations via XPath '{OperationsXpath}'");
			}
			var result = new List<Transfer>();
			foreach ( XmlNode operationNode in operations ) {
				HandleOperationNode(operationNode, result, commentFilter);
			}
			return result;
		}

		static void HandleOperationNode(
			XmlNode operationNode, List<Transfer> result, Func<string, bool> commentFilter) {
			var rawLastUpdateStr =
				operationNode.Attributes?["last_update"]?.Value ??
				throw new UnexpectedFormatException("Failed to retrieve 'last_update' operation attribute");
			// We expect that it's Moscow time, but no timezone provided
			// and for backward-compatibility we should use fixed value
			var lastUpdateStr = $"{rawLastUpdateStr}+3";
			if ( !DateTimeOffset.TryParse(lastUpdateStr, out var lastUpdate) ) {
				throw new UnexpectedFormatException($"Failed to parse DateTimeOffset from '{lastUpdateStr}'");
			}
			var operationTypeNodes  = operationNode.SelectNodes(OperationTypesXPath);
			if ( operationTypeNodes == null ) {
				throw new UnexpectedFormatException(
					$"Failed to retrieve operations via XPath '{OperationsXpath}/{OperationTypesXPath}'");
			}
			foreach ( XmlNode operationTypeNode in operationTypeNodes ) {
				HandleOperationTypeNode(operationTypeNode, lastUpdate, result, commentFilter);
			}
		}

		static void HandleOperationTypeNode(
			XmlNode operationTypeNode, DateTimeOffset lastUpdate, List<Transfer> result,
			Func<string, bool> commentFilter) {
			var operationType = operationTypeNode.Attributes?["oper_type"]?.Value ?? string.Empty;
			var commentNode   = operationTypeNode.SelectSingleNode(CommentXpath);
			if ( commentNode == null ) {
				throw new UnexpectedFormatException(
					$"Failed to retrieve operation details via XPath " +
					$"'{OperationsXpath}/{OperationTypesXPath}/{CommentXpath}'");
			}
			var comment = commentNode.Attributes?["comment"]?.Value ?? string.Empty;
			if ( !commentFilter(comment) ) {
				return;
			}
			var pCodeNodes = commentNode.SelectNodes(PCodesXpath);
			if ( pCodeNodes == null ) {
				throw new UnexpectedFormatException(
					$"Failed to retrieve operation details via XPath " +
					$"'{OperationsXpath}/{OperationTypesXPath}/{CommentXpath}/{PCodesXpath}'");
			}
			var nonZeroVolumeCodes = ParseNonZeroVolumeCodes(pCodeNodes);
			var fullName           = $"{operationType} {comment.TrimEnd()}";
			switch ( nonZeroVolumeCodes.Count ) {
				case 0:
					throw new UnexpectedFormatException(
						$"No valid values for '{fullName}' operation");
				case > 1:
					throw new UnexpectedFormatException(
						$"Too much valid values for '{fullName}' operation");
				default:
					var (currency, amount) = nonZeroVolumeCodes[0];
					result.Add(new(lastUpdate, fullName, currency, amount));
					break;
			}
		}

		static IReadOnlyList<(string, decimal)> ParseNonZeroVolumeCodes(XmlNodeList pCodeNodes) {
			var nonZeroVolumeCodes = new List<(string, decimal)>();
			foreach ( XmlNode pCode in pCodeNodes ) {
				var volumeStr = pCode.Attributes?["volume"]?.Value ?? string.Empty;
				if ( string.IsNullOrEmpty(volumeStr) ) {
					continue;
				}
				var code = pCode.Attributes?["p_code"]?.Value ?? string.Empty;
				if ( !decimal.TryParse(volumeStr, out var volume) ) {
					throw new UnexpectedFormatException($"Failed to parse operation volume from '{volumeStr}'");
				}
				nonZeroVolumeCodes.Add((code, volume));
			}
			return nonZeroVolumeCodes;
		}
	}
}