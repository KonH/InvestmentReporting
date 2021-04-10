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
				var name     = row.Cell("A").GetString().Trim();
				var isin     = row.Cell("AK").GetString().Trim();
				var type     = row.Cell("CV").GetString().Trim();
				var category = type.Contains("обл") ? "Bond" : "Share";
				result.Add(new(isin, name, category));
			}
			return result;
		}
	}
}