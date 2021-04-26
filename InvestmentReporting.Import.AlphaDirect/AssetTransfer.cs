using System;

namespace InvestmentReporting.Import.AlphaDirect {
	public record AssetTransfer(DateTimeOffset Date, string Name, int Count);
}