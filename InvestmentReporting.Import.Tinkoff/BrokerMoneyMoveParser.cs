using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;
using InvestmentReporting.Import.Exceptions;

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class BrokerMoneyMoveParser {
		public IReadOnlyCollection<Transfer> ReadIncomeTransfers(IXLWorkbook report) =>
			ReadTransfers(
				report,
				operation => operation == "Пополнение счета",
				(t, _) => t,
				income: true);

		public IReadOnlyCollection<Transfer> ReadDividendTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadCouponTransfers(IXLWorkbook report) =>
			ReadTransfers(
				report,
				operation => operation == "Выплата купонов",
				(t, c) => $"{t} {c}",
				income: true);

		public IReadOnlyCollection<Transfer> ReadRedemptionTransfers(IXLWorkbook report) =>
			ReadTransfers(
				report,
				operation => operation == "Погашение облигации",
				(t, c) => $"{t} {c}",
				income: true);

		public IReadOnlyCollection<Transfer> ReadExpenseTransfers(IXLWorkbook report) =>
			ReadTransfers(
				report,
				operation => operation == "Вывод средств",
				(t, _) => t,
				income: false);

		IReadOnlyCollection<Transfer> ReadTransfers(
			IXLWorkbook report, Func<string, bool> filter, Func<string, string, string> commentFactory, bool income) {
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
				var labelRow = report.FindRows(r => r.RowNumber() == header.Address.RowNumber + 1).First();
				var dateColumn = "A";
				var timeColumn = TableHelper.LookupColumnLetter(labelRow, "Время совершения");
				var operationColumn = TableHelper.LookupColumnLetter(labelRow, "Операция");
				var commentColumn = TableHelper.LookupColumnLetter(labelRow, "Примечание");
				var incomeColumn = TableHelper.LookupColumnLetter(labelRow, "Сумма зачисления");
				var expenseColumn = TableHelper.LookupColumnLetter(labelRow, "Сумма списания");
				foreach ( var row in rows ) {
					var rawDateCell = GetCellAtRowOrAbove(report, row.RowNumber(), dateColumn);
					var rawTimeCell = GetCellAtRowOrAbove(report, row.RowNumber(), timeColumn);
					var operation  = row.Cell(operationColumn).GetString().Trim();
					if ( !filter(operation) ) {
						continue;
					}
					var comment = row.Cell(commentColumn).GetString().Trim();
					operation = commentFactory(operation, comment);
					var sumLetter = income ? incomeColumn : expenseColumn;
					var sum    = row.Cell(sumLetter).GetDecimal();
					// We expect that it's Moscow time, but no timezone provided
					// and for backward-compatibility we should use fixed value
					var dateDt   = rawDateCell.GetDateTimeExact("dd.MM.yyyy", "MM/dd/yyyy hh:mm:ss");
					var date     = new DateTimeOffset(dateDt, TimeSpan.FromHours(3));
					var timeDt   = rawTimeCell.GetDateTime();
					var time     = new DateTimeOffset(timeDt, TimeSpan.FromHours(3));
					var fullDate = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Offset);
					sum = income ? sum : -sum;
					result.Add(new(fullDate, operation, currency, sum));
				}
			}
			return result;
		}

		IXLCell GetCellAtRowOrAbove(IXLWorkbook report, int row, string column) {
			var currentRowCell = report.FindCells(c => (c.Address.ColumnLetter == column) && (c.Address.RowNumber == row)).Single();
			if ( !string.IsNullOrEmpty(currentRowCell.GetString().Trim()) ) {
				return currentRowCell;
			}
			var aboveCells = report.FindCells(c => (c.Address.ColumnLetter == column) && (c.Address.RowNumber < row))
				.Reverse()
				.ToArray();
			foreach ( var aboveCell in aboveCells ) {
				if ( !string.IsNullOrEmpty(aboveCell.GetString()?.Trim()) ) {
					return aboveCell;
				}
			}
			throw new UnexpectedFormatException($"Failed to find non-empty cell at {column}{row} or above");
		}
	}
}