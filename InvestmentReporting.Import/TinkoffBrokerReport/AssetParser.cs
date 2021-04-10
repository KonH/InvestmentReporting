using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class AssetParser {
		public IReadOnlyCollection<Asset> ReadAssets(IXLWorkbook report) {
			var result       = new List<Asset>();
			var tradesHeader = report.Search("4.1 Информация о ценных бумагах").Single();
			var nextHeader   = report.Search("4.2 Информация об инструментах, не квалифицированных в качестве ценной бумаги").Single();
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
				var name     = cells.First(c => c.column == "A").value;
				var isin     = cells.First(c => c.column == "AK").value;
				var type     = cells.First(c => c.column == "CV").value;
				var category = type.Contains("обл") ? "Bond" : "Share";
				result.Add(new(isin, name, category));
			}
			return result;
		}
	}
}