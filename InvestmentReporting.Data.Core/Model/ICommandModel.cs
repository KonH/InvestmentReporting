using System;

namespace InvestmentReporting.Data.Core.Model {
	public interface ICommandModel {
		DateTimeOffset Date { get; }
		string         User { get; }
	}
}