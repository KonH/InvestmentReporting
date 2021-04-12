using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Logic;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Network;

namespace InvestmentReporting.MarketService.Services {
	public sealed class MarketMetadataCollector {
		readonly ILogger        _logger;
		readonly IStateManager  _stateManager;
		readonly ApiKeyProvider _apiKeyProvider;

		public MarketMetadataCollector(
			ILogger<MarketMetadataCollector> logger, IStateManager stateManager, ApiKeyProvider apiKeyProvider) {
			_logger         = logger;
			_stateManager   = stateManager;
			_apiKeyProvider = apiKeyProvider;
		}

		public async Task Collect(CancellationToken ct) {
			var states = await _stateManager.ReadStates(DateTimeOffset.MaxValue);
			var assetIsins = states
				.SelectMany(s => s.Value.Brokers.SelectMany(b => b.Inventory))
				.Select(a => a.Isin)
				.ToArray();
			_logger.LogTrace($"Found {assetIsins.Length} assets in {states.Count} states");

			_logger.LogTrace($"API key: {_apiKeyProvider.ApiKey}");

			var connection = ConnectionFactory.GetSandboxConnection(_apiKeyProvider.ApiKey);
			var context    = connection.Context;
			var stocks     = await context.MarketStocksAsync();
			var bonds      = await context.MarketBondsAsync();
			var etfs       = await context.MarketEtfsAsync();
			var instruments = stocks.Instruments
				.Concat(bonds.Instruments)
				.Concat(etfs.Instruments)
				.ToArray();
			_logger.LogTrace($"Found {instruments.Length} instruments");
			var instrumentMap = instruments.ToDictionary(instrument => instrument.Isin, instrument => instrument);
			foreach ( var isin in assetIsins ) {
				if ( instrumentMap.TryGetValue(isin, out var instrument) ) {
					_logger.LogTrace($"Found instrument for ISIN '{isin}': {instrument}");
				} else {
					_logger.LogWarning($"Instrument not found for ISIN: '{isin}'");
				}
			}
		}
	}
}