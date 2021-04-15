using System;
using System.Threading;
using System.Threading.Tasks;
using InvestmentReporting.Market.Logic;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Market.UseCase {
	public sealed class SynchronizationUseCase {
		readonly ILogger                 _logger;
		readonly MarketMetadataCollector _metadataCollector;
		readonly MarketCandleCollector   _candleCollector;

		public SynchronizationUseCase(
			ILogger<SynchronizationUseCase> logger,
			MarketMetadataCollector metadataCollector, MarketCandleCollector candleCollector) {
			_logger            = logger;
			_metadataCollector = metadataCollector;
			_candleCollector   = candleCollector;
		}

		public async Task Handle(CancellationToken ct) {
			_logger.LogInformation("Synchronization started");
			try {
				await _metadataCollector.Collect(ct);
				await _candleCollector.Collect(ct);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
			}
			_logger.LogInformation("Synchronization finished");
		}
	}
}