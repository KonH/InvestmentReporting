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
			foreach ( var row in rows ) {
				// We expect that it's Moscow time, but no timezone provided
				// and for backward-compatibility we should use fixed value
				var dateDt   = row.Cell("H").GetDateTimeExact("dd.MM.yyyy", "MM/dd/yyyy hh:mm:ss");
				var date     = new DateTimeOffset(dateDt, TimeSpan.FromHours(3));
				var timeDt   = row.Cell("L").GetDateTime();
				var time     = new DateTimeOffset(timeDt, TimeSpan.FromHours(3));
				var fullDate = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, time.Offset);
				var type     = row.Cell("AB").GetString().Trim();
				var buy      = (type == "Покупка");
				var name     = row.Cell("AF").GetString().Trim();
				// Skip money transfer deals
				if ( name.EndsWith("_TOM") ) {
					continue;
				}
				var isin     = assets.Single(a => a.Name == name).Isin;
				var count = (int)row.Cell("BB").GetDouble();
				count = buy ? count : -count;
				var currency  = row.Cell("AW").GetString().Trim();
				var sum       = row.Cell("BQ").GetDecimal();
				var brokerFee = row.Cell("CC").GetDecimal();
				var fee       = brokerFee;
				result.Add(new(fullDate, isin, name, count, currency, sum, fee));
			}
			return result;
		}
	}
}