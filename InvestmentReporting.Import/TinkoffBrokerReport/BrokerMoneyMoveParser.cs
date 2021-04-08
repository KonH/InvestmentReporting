using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class BrokerMoneyMoveParser {
		public IReadOnlyCollection<Transfer> ReadIncomeTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadDividendTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadCouponTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadExpenseTransfers(IXLWorkbook report) =>
			throw new NotImplementedException();
	}
}