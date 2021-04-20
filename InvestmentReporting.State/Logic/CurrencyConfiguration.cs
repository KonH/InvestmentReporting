using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Logic {
	public sealed class CurrencyConfiguration {
		readonly Dictionary<CurrencyCode, CurrencyFormat> _currencies = new() {
			[new("RUB")] = new("{sign}{value} ₽"),
			[new("USD")] = new("{sign}${value}"),
			[new("EUR")] = new("{sign}€{value}"),
		};

		public IReadOnlyCollection<CurrencyCode> GetAll() => _currencies.Keys.ToArray();

		public bool HasCurrency(CurrencyCode code) => _currencies.ContainsKey(code);

		public CurrencyFormat GetFormat(CurrencyCode code) =>
			_currencies.TryGetValue(code, out var format) ? format : new("{sign}{value}");
	}
}