using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.Tinkoff {
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
			var labelRow = report.FindRows(r => r.RowNumber() == tradesHeader.Address.RowNumber + 1).First();
			var dateColumn = TableHelper.LookupColumnLetter(labelRow, "Дата заключения");
			var timeColumn = TableHelper.LookupColumnLetter(labelRow, "Время");
			var typeColumn = TableHelper.LookupColumnLetter(labelRow, "Вид сделки");
			var nameColumn = TableHelper.LookupColumnLetter(labelRow, "Сокращенное наименование актива");
			var currencyColumn = TableHelper.LookupColumnLetter(labelRow, "Валюта цены");
			var countColumn = TableHelper.LookupColumnLetter(labelRow, "Количество");
			var sumColumn = TableHelper.LookupColumnLetter(labelRow, "Сумма сделки");
			var feeColumn = TableHelper.LookupColumnLetter(labelRow, "Комиссия брокера");
			foreach ( var row in rows ) {
				var dateCell = row.Cell(dateColumn);
				if ( IsPageSeparator(dateCell.GetString()) ) {
					continue;
				}
				// We expect that it's Moscow time, but no timezone provided
				// and for backward-compatibility we should use fixed value
				var dateDt   = dateCell.GetDateTimeExact("dd.MM.yyyy", "MM/dd/yyyy hh:mm:ss");
				var date     = new DateTimeOffset(dateDt, TimeSpan.FromHours(3));
				var timeDt   = row.Cell(timeColumn).GetDateTime();
				var time     = new DateTimeOffset(timeDt, TimeSpan.FromHours(3));
				var fullDate = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Offset);
				var type     = row.Cell(typeColumn).GetString().Trim();
				if ( IsTemporaryType(type) ) {
					continue;
				}
				var buy = (type == "Покупка");
				var name = row.Cell(nameColumn).GetString().Trim();
				// Skip money transfer deals
				if ( name.EndsWith("_TOM") ) {
					continue;
				}
				var isin     = assets.Single(a => a.Name == name).Isin;
				var count = (int)row.Cell(countColumn).GetDouble();
				count = buy ? count : -count;
				var currency  = row.Cell(currencyColumn).GetString().Trim();
				var sum       = row.Cell(sumColumn).GetDecimal();
				var brokerFee = row.Cell(feeColumn).GetDecimal();
				var fee       = brokerFee;
				result.Add(new(fullDate, isin, name, count, currency, sum, fee));
			}
			return result;
		}

		public IReadOnlyCollection<Exchange> ReadExchanges(IXLWorkbook report) {
			var result       = new List<Exchange>();
			var tradesHeader = report.Search("1.1 Информация о совершенных и исполненных сделках на конец отчетного периода").Single();
			var nextHeader   = report.Search("1.2 Информация о неисполненных сделках на конец отчетного периода").Single();
			var startRow     = tradesHeader.Address.RowNumber + 2;
			var endRow       = nextHeader.Address.RowNumber - 1;
			var rows = report.FindRows(r =>
				(r.RowNumber() >= startRow) &&
				(r.RowNumber() <= endRow));
			var labelRow = report.FindRows(r => r.RowNumber() == tradesHeader.Address.RowNumber + 1).First();
			var dateColumn = TableHelper.LookupColumnLetter(labelRow, "Дата заключения");
			var timeColumn = TableHelper.LookupColumnLetter(labelRow, "Время");
			var typeColumn = TableHelper.LookupColumnLetter(labelRow, "Вид сделки");
			var nameColumn = TableHelper.LookupColumnLetter(labelRow, "Сокращенное наименование актива");
			var currencyColumn = TableHelper.LookupColumnLetter(labelRow, "Валюта цены");
			var countColumn = TableHelper.LookupColumnLetter(labelRow, "Количество");
			var sumColumn = TableHelper.LookupColumnLetter(labelRow, "Сумма сделки");
			var feeColumn = TableHelper.LookupColumnLetter(labelRow, "Комиссия брокера");
			foreach ( var row in rows ) {
				var dateCell = row.Cell(dateColumn);
				if ( IsPageSeparator(dateCell.GetString()) ) {
					continue;
				}
				var dateDt   = dateCell.GetDateTimeExact("dd.MM.yyyy", "MM/dd/yyyy hh:mm:ss");
				var date     = new DateTimeOffset(dateDt, TimeSpan.FromHours(3));
				var timeDt   = row.Cell(timeColumn).GetDateTime();
				var time     = new DateTimeOffset(timeDt, TimeSpan.FromHours(3));
				var fullDate = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Offset);
				var type     = row.Cell(typeColumn).GetString().Trim();
				if ( IsTemporaryType(type) ) {
					continue;
				}
				var buy  = (type == "Покупка");
				var name = row.Cell(nameColumn).GetString().Trim();
				if ( !name.EndsWith("_TOM") ) {
					continue;
				}
				var count = (int)row.Cell(countColumn).GetDouble();
				count = buy ? count : -count;
				var fromCurrency = row.Cell(currencyColumn).GetString().Trim();
				var toCurrency   = name[..3];
				var sum          = row.Cell(sumColumn).GetDecimal();
				var brokerFee    = row.Cell(feeColumn).GetDecimal();
				var fee          = brokerFee;
				result.Add(new(fullDate, fromCurrency, toCurrency, count, sum, fee));
			}
			return result;
		}

		bool IsPageSeparator(string checkValue) {
			return string.IsNullOrWhiteSpace(checkValue) || checkValue.StartsWith("Дата");
		}

		// Potential temporary operations, which can be skipped (?)
		bool IsTemporaryType(string type) =>
			type.StartsWith("РЕПО ");
	}
}