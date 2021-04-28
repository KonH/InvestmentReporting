using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Tinkoff.Trading.OpenApi.Network;

namespace InvestmentReporting.Market.Logic {
	[ExcludeFromCodeCoverage]
	public sealed class MarketCandleCollector {
		readonly IApiKeyProvider _apiKeyProvider;

		readonly MarkerAssetCandleCollector    _assetCollector;
		readonly MarketCurrencyCandleCollector _currencyCollector;

		public MarketCandleCollector(
			IApiKeyProvider apiKeyProvider,
			MarkerAssetCandleCollector assetCollector, MarketCurrencyCandleCollector currencyCollector) {
			_apiKeyProvider    = apiKeyProvider;
			_assetCollector    = assetCollector;
			_currencyCollector = currencyCollector;
		}

		public async Task Collect(CancellationToken ct) {
			ct.ThrowIfCancellationRequested();
			var connection = ConnectionFactory.GetSandboxConnection(_apiKeyProvider.ApiKey);
			var context    = connection.Context;
			await _assetCollector.Collect(context);
			await _currencyCollector.Collect(context);
		}
	}
}