using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class TradeParser {
		public IReadOnlyCollection<Trade> ReadTrades(IXLWorkbook report, IReadOnlyCollection<Asset> assets) {
			var result       = new List<Trade>();
			var tradesHeader = report.Search("1.1 Информация о совершенных и исполненных сделках на конец отчетного периода").Single();
			var nextHeader   = report.Search("1.2 Информация о неисполненных сделках на конец отчетного периода").Single();
			var startRow     = tradesHeader.Address.RowNumber + 2;
			var endRow       = nextHeader.Address.RowNumber - 1;
			var rows = report.FindRows(r =>
				(r.RowNumber() >= startRow) &&
				(r.RowNumber() <= endRow));
			foreach ( var row in rows ) {
				var cells = row.CellsUsed()
					.Select(c => (column: c.Address.ColumnLetter, value: c.Value.ToString()?.Trim() ?? string.Empty))
					.Where(c => !string.IsNullOrEmpty(c.value))
					.ToArray();
				var rawDateStr = cells.First(c => c.column == "H").value;
				var rawTimeStr = cells.First(c => c.column == "L").value;
				// We expect that it's Moscow time, but no timezone provided
				// and for backward-compatibility we should use fixed value
				var dateStr = $"{rawDateStr}+3";
				if ( !DateTimeOffset.TryParse(dateStr, out var date) ) {
					throw new UnexpectedFormatException($"Failed to parse DateTimeOffset from '{dateStr}'");
				}
				var timeStr = $"{rawTimeStr}+3";
				if ( !DateTimeOffset.TryParse(timeStr, out var time) ) {
					throw new UnexpectedFormatException($"Failed to parse DateTimeOffset from '{rawTimeStr}'");
				}
				var fullDate = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Offset);
				var type     = cells.First(c => c.column == "AB").value;
				var buy      = (type == "Покупка");
				var name     = cells.First(c => c.column == "AF").value;
				var isin     = assets.Single(a => a.Name == name).Isin;
				var category = assets.Single(a => a.Name == name).Category;
				var countStr = cells.First(c => c.column == "BB").value;
				if ( !int.TryParse(countStr, out var count) ) {
					throw new UnexpectedFormatException($"Failed to parse count from '{countStr}'");
				}
				count = buy ? count : -count;
				var currency = cells.First(c => c.column == "AW").value;
				var sumStr   = cells.First(c => c.column == "BQ").value;
				if ( !decimal.TryParse(sumStr, out var sum) ) {
					throw new UnexpectedFormatException($"Failed to parse sum from '{sumStr}'");
				}
				var brokerFeeStr = cells.First(c => c.column == "CC").value;
				if ( !decimal.TryParse(brokerFeeStr, out var brokerFee) ) {
					throw new UnexpectedFormatException($"Failed to parse broker fee from '{brokerFeeStr}'");
				}
				var marketFeeStr = cells.First(c => c.column == "CL").value;
				if ( !decimal.TryParse(marketFeeStr, out var marketFee) ) {
					throw new UnexpectedFormatException($"Failed to parse market fee from '{marketFeeStr}'");
				}
				var clearCenterFeeStr = cells.First(c => c.column == "CW").value;
				if ( !decimal.TryParse(clearCenterFeeStr, out var clearCenterFee) ) {
					throw new UnexpectedFormatException($"Failed to parse clear center fee from '{clearCenterFeeStr}'");
				}
				var fee = brokerFee + marketFee + clearCenterFee;
				result.Add(new(fullDate, isin, name, category, count, currency, sum, fee));
			}
			return result;
		}
	}
}