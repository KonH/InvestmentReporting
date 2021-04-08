using System;
using System.Collections.Generic;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.TinkoffBrokerReport {
	public sealed class BrokerMoneyMoveParser {
		public IReadOnlyCollection<Transfer> ReadIncomeTransfers(object report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadDividendTransfers(object report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadCouponTransfers(object report) =>
			throw new NotImplementedException();

		public IReadOnlyCollection<Transfer> ReadExpenseTransfers(object report) =>
			throw new NotImplementedException();
	}
}