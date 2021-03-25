using System;

namespace InvestmentReporting.Import.Exceptions {
	public sealed class UnexpectedFormatException : Exception {
		public UnexpectedFormatException(string message) : base(message) {}
	}
}