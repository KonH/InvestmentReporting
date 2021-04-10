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
						.Select(c => c.Value.ToString()?.Trim())
						.Where(value => !string.IsNullOrEmpty(value))
						.ToArray();
					var rawDateStr = cells[0];
					var rawTimeStr = cells[1];
					var operation  = cells[3]?.Trim() ?? string.Empty;
					if ( !filter(operation) ) {
						continue;
					}
					var sumCell = income ? 4 : 5;
					var sumStr  = cells[sumCell];

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
	}
}