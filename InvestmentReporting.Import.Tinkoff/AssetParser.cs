using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class AssetParser {
		public IReadOnlyCollection<Asset> ReadAssets(IXLWorkbook report) {
			var result       = new List<Asset>();
			var tradesHeader = report.Search("4.1 Информация о ценных бумагах").Single();
			var nextHeader   = report.Search("4.2 Информация об инструментах, не квалифицированных в качестве ценной бумаги").Single();
			var potentialLabelCell = report
				.FindCells(c =>
					(c.Address.ColumnLetter == "A") &&
					(c.Address.RowNumber == tradesHeader.Address.RowNumber + 1))
				.First();
			var offset = !string.IsNullOrWhiteSpace(potentialLabelCell.GetString()) ? 1 : 2;
			var startRow = tradesHeader.Address.RowNumber + offset + 1;
			var endRow = nextHeader.Address.RowNumber - 1;
			var rows = report.FindRows(r =>
				(r.RowNumber() >= startRow) &&
				(r.RowNumber() <= endRow));
			var labelRow = report.FindRows(r => r.RowNumber() == tradesHeader.Address.RowNumber + offset).First();
			var nameColumn = "A";
			var isinColumn = TableHelper.LookupColumnLetter(labelRow, "ISIN");
			var typeColumn = TableHelper.LookupColumnLetter(labelRow, "Тип");
			foreach ( var row in rows ) {
				var name     = row.Cell(nameColumn).GetString().Trim();
				var isin     = row.Cell(isinColumn).GetString().Trim();
				var type     = row.Cell(typeColumn).GetString().Trim();
				var category = type.Contains("обл") ? "Bond" : "Share";
				result.Add(new(isin, name, category));
			}
			return result;
		}
	}
}