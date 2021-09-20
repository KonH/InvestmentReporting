using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Import.Dto;

namespace InvestmentReporting.Import.Tinkoff {
	public sealed class SplitDetector {
		public IReadOnlyCollection<SplitCase> DetectSplitCases(IReadOnlyCollection<Trade> trades, IReadOnlyCollection<AssetState> assetStates) => assetStates
			.Select(s => {
				var realCount = s.OutCount;
				var actualCount = trades
					.Where(t => t.Name == s.Name)
					.Sum(t => t.Count);
				return new SplitCase(s.Name, realCount - actualCount);
			})
			.Where(c => (c.Diff > 0))
			.ToArray();
	}
}