using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;

namespace InvestmentReporting.Market.Logic {
	public sealed class CurrencyManager {
		readonly IStateManager _stateManager;

		public CurrencyManager(IStateManager stateManager) {
			_stateManager = stateManager;
		}

		public IReadOnlyCollection<CurrencyCode> GetAll() {
			return _stateManager.ReadStates()
				.SelectMany(s => s.Value.Currencies)
				.Select(c => c.Code)
				.Distinct()
				.ToArray();
		}
	}
}