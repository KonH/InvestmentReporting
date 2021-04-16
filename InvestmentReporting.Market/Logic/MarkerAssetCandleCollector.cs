using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tinkoff.Trading.OpenApi.Models;
using Tinkoff.Trading.OpenApi.Network;

namespace InvestmentReporting.Market.Logic {
	public sealed class MarkerAssetCandleCollector {
		readonly ILogger _logger;

		readonly MetadataManager         _metadataManager;
		readonly AssetPriceManager       _priceManager;
		readonly AssetIntervalCalculator _intervalCalculator;

		public MarkerAssetCandleCollector(
			ILogger<MarkerAssetCandleCollector> logger, MetadataManager metadataManager,
			AssetPriceManager priceManager, AssetIntervalCalculator intervalCalculator) {
			_logger             = logger;
			_metadataManager    = metadataManager;
			_priceManager       = priceManager;
			_intervalCalculator = intervalCalculator;
		}

		public async Task Collect(SandboxContext context) {
			foreach ( var metadata in _metadataManager.GetAll() ) {
				var isin     = metadata.Isin;
				var interval = _intervalCalculator.TryCalculateRequiredInterval(isin);
				if ( interval == null ) {
					continue;
				}
				var (startDate, endDate) = interval;
				var candles = await context.MarketCandlesAsync(
					metadata.Figi, startDate, endDate, CandleInterval.Day);
				_logger.LogTrace($"Found asset candles: {string.Join("\n", candles.Candles.Select(c => c.ToString()))}");
				if ( candles.Candles.Count > 0 ) {
					await _priceManager.AddOrAppendCandles(isin, candles);
				}
			}
		}
	}
}