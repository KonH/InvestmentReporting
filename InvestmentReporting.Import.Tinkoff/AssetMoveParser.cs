using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class AssetMoveParser {
		public IReadOnlyCollection<AssetState> ReadAssetStates(IXLWorkbook report) {
			var result     = new List<AssetState>();
			var header     = report.Search("3.1 Движение по ценным бумагам инвестора").Single();
			var nextHeader = report.Search("3.2 Движение по производным финансовым инструментам").Single();
			var startRow   = header.Address.RowNumber + 2;
			var endRow     = nextHeader.Address.RowNumber - 1;
			var rows = report
				.FindRows(r =>
					(r.RowNumber() >= startRow) &&
					(r.RowNumber() <= endRow))
				.ToArray();
			var labelRow       = report.FindRows(r => r.RowNumber() == header.Address.RowNumber + 1).First();
			var nameColumn     = TableHelper.LookupColumnLetter(labelRow, "Сокращенное наименование актива");
			var inCountColumn  = TableHelper.LookupColumnLetter(labelRow, "Входящий остаток");
			var incColumn      = TableHelper.LookupColumnLetter(labelRow, "Зачисление");
			var decColumn      = TableHelper.LookupColumnLetter(labelRow, "Списание");
			var outCountColumn = TableHelper.LookupColumnLetter(labelRow, "Исходящий остаток");
			foreach ( var row in rows ) {
				var name = row.Cell(nameColumn).GetString().Trim();
				var inCount = TryGetValue(row.Cell(inCountColumn));
				var inc = TryGetValue(row.Cell(incColumn));
				var dec = TryGetValue(row.Cell(decColumn));
				var outCount = TryGetValue(row.Cell(outCountColumn));
				result.Add(new(name, inCount, inc, dec, outCount));
			}
			return result;
		}

		int TryGetValue(IXLCell cell) {
			try {
				return (int)cell.GetDouble();
			} catch {
				return 0;
			}
		}
	}
}