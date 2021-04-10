using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class BrokerMoneyMoveParser {
		public IReadOnlyCollection<Transfer> ReadIncomeTransfers(IXLWorkbook report) =>
			ReadTransfers(
				report,
				operation => operation == "Пополнение счета",
				income: true);

		public IReadOnlyCollection<Transfer> ReadDividendTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadCouponTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadExpenseTransfers(IXLWorkbook report) =>
			ReadTransfers(
				report,
				operation => operation == "Вывод средств",
				income: false);

		IReadOnlyCollection<Transfer> ReadTransfers(IXLWorkbook report, Func<string, bool> filter, bool income) {
			var result           = new List<Transfer>();
			var operationsHeader = report.Search("2. Операции с денежными средствами").Single();
			var nextCatHeader    = report.Search("3.1 Движение по ценным бумагам инвестора").Single();
			var currencies       = new [] { "RUB", "USD", "EUR" };
			var startCurrencyRow = operationsHeader.Address.RowNumber;
			var endCurrencyRow   = nextCatHeader.Address.RowNumber;
			var currencyHeaders = currencies
				.Select(currency => {
					var allCells = report.Search(currency)
						.Where(cell =>
							(cell.Address.RowNumber > startCurrencyRow) &&
							(cell.Address.RowNumber < endCurrencyRow))
						.ToArray();
					return (allCells.Length > 1) ? allCells.Last() : null;
				})
				.Where(header => (header != null))
				.Select(header => header!)
				.ToArray();
			for ( var i = 0; i < currencyHeaders.Length; i++ ) {
				var header     = currencyHeaders[i];
				var nextHeader = (i < currencyHeaders.Length - 1) ? currencyHeaders[i + 1] : nextCatHeader;
				var startRow   = header.Address.RowNumber + 2;
				var endRow     = nextHeader.Address.RowNumber - 1;
				var rows       = report.FindRows(r =>
					(r.RowNumber() >= startRow) &&
					(r.RowNumber() <= endRow));
				var currency = header.Value.ToString()?.Trim() ?? string.Empty;
				foreach ( var row in rows ) {
					var cells = row.CellsUsed()
						.Select(c => (column: c.Address.ColumnLetter, value: c.Value.ToString()?.Trim()))
						.Where(t => !string.IsNullOrEmpty(t.Item2))
						.Select(t => (t.column, value: t.value!))
						.ToArray();
					var rawDateStr = GetStrAtRowOrAbove(row.RowNumber(), "A", cells, report);
					var rawTimeStr = GetStrAtRowOrAbove(row.RowNumber(), "N", cells, report);
					var operation  = cells.First(c => c.column == "AZ").value;
					if ( !filter(operation) ) {
						continue;
					}
					var sumLetter = income ? "BV" : "CS";
					var sumStr  = cells.First(c => c.column == sumLetter).value;

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
					if ( !decimal.TryParse(sumStr, out var sum) ) {
						throw new UnexpectedFormatException($"Failed to parse sum from '{sumStr}'");
					}
					sum = income ? sum : -sum;
					result.Add(new(fullDate, operation, currency, sum));
				}
			}
			return result;
		}

		string GetStrAtRowOrAbove(int row, string column, (string column, string value)[] cells, IXLWorkbook report) {
			var currentRowCell = cells.FirstOrDefault(c => c.column == column);
			if ( !string.IsNullOrEmpty(currentRowCell.column) ) {
				return currentRowCell.value;
			}
			var aboveCells = report.FindCells(c => (c.Address.ColumnLetter == column) && (c.Address.RowNumber < row))
				.Reverse()
				.ToArray();
			foreach ( var aboveCell in aboveCells ) {
				var value = aboveCell.Value.ToString()?.Trim() ?? string.Empty;
				if ( !string.IsNullOrEmpty(value) ) {
					return value;
				}
			}
			return string.Empty;
		}
	}
}