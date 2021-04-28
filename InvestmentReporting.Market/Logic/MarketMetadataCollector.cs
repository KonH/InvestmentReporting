using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace InvestmentReporting.Market.Logic {
	[ExcludeFromCodeCoverage]
	public sealed class MarketMetadataCollector {
		readonly ILogger         _logger;
		readonly IApiKeyProvider _apiKeyProvider;
		readonly IsinProvider    _isinProvider;
		readonly MetadataManager _metadataManager;

		public MarketMetadataCollector(
			ILogger<MarketMetadataCollector> logger, IApiKeyProvider apiKeyProvider, IsinProvider isinProvider,
			MetadataManager metadataManager) {
			_logger          = logger;
			_isinProvider    = isinProvider;
			_apiKeyProvider  = apiKeyProvider;
			_metadataManager = metadataManager;
		}

		public async Task Collect(CancellationToken ct) {
			var assetIsins = _isinProvider.CollectRequiredIsins();

			ct.ThrowIfCancellationRequested();
			var connection = ConnectionFactory.GetSandboxConnection(_apiKeyProvider.ApiKey);
			var context    = connection.Context;
			var stocks     = await context.MarketStocksAsync();
			var bonds      = await context.MarketBondsAsync();
			var etfs       = await context.MarketEtfsAsync();
			var instruments = stocks.Instruments
				.Concat(bonds.Instruments)
				.Concat(etfs.Instruments)
				.ToArray();
			_logger.LogTrace($"Found {instruments.Length} instruments from API");

			ct.ThrowIfCancellationRequested();
			var instrumentsByIsin = instruments
				.GroupBy(instrument => instrument.Isin)
				.ToDictionary(group => group.Key, group => group.ToArray());
			foreach ( var isin in assetIsins ) {
				var instrument = GetInstrumentByIsin(instrumentsByIsin, isin);
				if ( instrument == null ) {
					continue;
				}
				await _metadataManager.TryAdd(instrument);
			}
		}

		MarketInstrument? GetInstrumentByIsin(Dictionary<string, MarketInstrument[]> instrumentsByIsin, string isin) {
			if ( !instrumentsByIsin.TryGetValue(isin, out var instruments) ) {
				_logger.LogError($"No instrument found for ISIN '{isin}'");
				return null;
			}
			if ( instruments.Length > 1 ) {
				var details = string.Join("; ", instruments.Select(inst => inst.ToString()));
				_logger.LogWarning($"Found multiple instruments for ISIN '{isin}', last one will be used: {details}");
			}
			var instrument = instruments.Last();
			_logger.LogTrace($"Found instrument for ISIN '{isin}': {instrument}");
			return instrument;
		}
	}
}