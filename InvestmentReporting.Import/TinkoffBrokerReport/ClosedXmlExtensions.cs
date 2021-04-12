using System;
using ClosedXML.Excel;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public static class ClosedXmlExtensions {
		public static DateTime GetDateTimeExact(this IXLCell cell, string format) {
			var str = cell.GetString();
			return DateTime.ParseExact(str, format, null);
		}

		public static decimal GetDecimal(this IXLCell cell) {
			var str = cell.GetString().Replace(',', '.');
			return decimal.Parse(str);
		}
	}
}