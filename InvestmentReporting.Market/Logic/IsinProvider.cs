using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.Logic {
	public sealed class IsinProvider {
		readonly ILogger       _logger;
		readonly IStateManager _stateManager;

		public IsinProvider(ILogger<IsinProvider> logger, IStateManager stateManager) {
			_logger       = logger;
			_stateManager = stateManager;
		}

		public IReadOnlyCollection<AssetISIN> CollectRequiredIsins() {
			var states = _stateManager.ReadStates();
			var assetIsins = states
				.SelectMany(s => s.Value.Brokers.SelectMany(b => b.Inventory))
				.Select(a => a.Isin)
				.ToArray();
			_logger.LogTrace($"Found {assetIsins.Length} assets in {states.Count} states");
			return assetIsins;
		}
	}
}