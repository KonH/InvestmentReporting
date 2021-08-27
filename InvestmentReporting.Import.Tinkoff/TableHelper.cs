using System.Linq;
using ClosedXML.Excel;
using InvestmentReporting.Import.Exceptions;

namespace InvestmentReporting.Import.Tinkoff {
    static class TableHelper {
        public static string LookupColumnLetter(IXLRow row, string text) {
            var targetText = ToLettersOnlyString(text);
            var targetRow = row
                .Cells(c => ToLettersOnlyString(c.GetString()) == targetText)
                .FirstOrDefault();
            return targetRow?.Address.ColumnLetter ?? throw new UnexpectedFormatException($"Failed to find header for '{text}'");
        }

        static string ToLettersOnlyString(string input) => new(input.Where(char.IsLetter).ToArray());
    }
}