using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace InvestmentReporting.Market.Logic {
	public sealed class MarketCandleCollector {
		readonly ILogger         _logger;
		readonly IApiKeyProvider _apiKeyProvider;

		readonly MetadataManager    _metadataManager;
		readonly PriceManager       _priceManager;
		readonly IntervalCalculator _intervalCalculator;

		public MarketCandleCollector(
			ILogger<MarketCandleCollector> logger, IApiKeyProvider apiKeyProvider,
			MetadataManager metadataManager, PriceManager priceManager, IntervalCalculator intervalCalculator) {
			_logger             = logger;
			_apiKeyProvider     = apiKeyProvider;
			_metadataManager    = metadataManager;
			_priceManager       = priceManager;
			_intervalCalculator = intervalCalculator;
		}

		public async Task Collect(CancellationToken ct) {
			ct.ThrowIfCancellationRequested();
			var connection = ConnectionFactory.GetSandboxConnection(_apiKeyProvider.ApiKey);
			var context    = connection.Context;
			foreach ( var metadata in _metadataManager.GetAll() ) {
				var isin     = metadata.Isin;
				var interval = _intervalCalculator.TryCalculateRequiredInterval(isin);
				if ( interval == null ) {
					continue;
				}
				var (startDate, endDate) = interval;
				var candles = await context.MarketCandlesAsync(
					metadata.Figi, startDate, endDate, CandleInterval.Day);
				_logger.LogTrace($"Found candles: {string.Join("\n", candles.Candles.Select(c => c.ToString()))}");
				if ( candles.Candles.Count > 0 ) {
					await _priceManager.AddOrAppendCandles(isin, candles);
				}
			}
		}
	}
}