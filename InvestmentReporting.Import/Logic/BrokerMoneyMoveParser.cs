using System;
using System.Collections.Generic;
using System.Xml;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;

namespace InvestmentReporting.Import.Logic {
	public sealed class BrokerMoneyMoveParser {
		const string OperationsXpath =
			"Report[@Name=\"MyBroker\"]/*/Report[@Name=\"3_BrokerMoneyMove\"]/Tablix1/settlement_date_Collection/settlement_date/rn_Collection/rn";

		const string OperationTypesXPath = "oper_type[@oper_type=\"Перевод\"]";

		const string CommentXpath = "comment";

		const string PCodesXpath =
			"money_volume_begin1_Collection/money_volume_begin1/p_code_Collection/p_code/p_code";

		public IReadOnlyCollection<IncomeTransfer> ReadIncomeTransfers(XmlDocument report) {
			var operations = report.SelectNodes(OperationsXpath);
			if ( operations == null ) {
				throw new UnexpectedFormatException($"Failed to retrieve operations via XPath '{OperationsXpath}'");
			}
			var result = new List<IncomeTransfer>();
			foreach ( XmlNode operationNode in operations ) {
				HandleOperationNode(operationNode, result);
			}
			return result;
		}

		static void HandleOperationNode(XmlNode operationNode, List<IncomeTransfer> result) {
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
				HandleOperationTypeNode(operationTypeNode, lastUpdate, result);
			}
		}

		static void HandleOperationTypeNode(
			XmlNode operationTypeNode, DateTimeOffset lastUpdate, List<IncomeTransfer> result) {
			var operationType = operationTypeNode.Attributes?["oper_type"]?.Value ?? string.Empty;
			var commentNode   = operationTypeNode.SelectSingleNode(CommentXpath);
			if ( commentNode == null ) {
				throw new UnexpectedFormatException(
					$"Failed to retrieve operation details via XPath " +
					$"'{OperationsXpath}/{OperationTypesXPath}/{CommentXpath}'");
			}
			var comment    = commentNode.Attributes?["comment"]?.Value ?? string.Empty;
			var pCodeNodes = commentNode.SelectNodes(PCodesXpath);
			if ( pCodeNodes == null ) {
				throw new UnexpectedFormatException(
					$"Failed to retrieve operation details via XPath " +
					$"'{OperationsXpath}/{OperationTypesXPath}/{CommentXpath}/{PCodesXpath}'");
			}
			var nonZeroVolumeCodes = ParseNonZeroVolumeCodes(pCodeNodes);
			var fullName           = $"{operationType} {comment}";
			switch ( nonZeroVolumeCodes.Count ) {
				case 0:
					throw new UnexpectedFormatException(
						$"No valid values for '{fullName}' operation");
				case > 1:
					throw new UnexpectedFormatException(
						$"Too much valid values for '{fullName}' operation");
				default:
					var (currency, amount) = nonZeroVolumeCodes[0];
					result.Add(new IncomeTransfer(lastUpdate, fullName, currency, amount));
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