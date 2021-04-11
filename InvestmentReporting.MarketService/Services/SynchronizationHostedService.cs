using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MarketService.Services {
	public sealed class SynchronizationHostedService : IHostedService {
		readonly TimeSpan _interval = TimeSpan.FromDays(1);

		readonly ILogger                 _logger;
		readonly MarketMetadataCollector _metadataCollector;

		readonly CancellationTokenSource _cancellationTokenSource = new();

		readonly Timer _timer;

		public SynchronizationHostedService(ILogger<SynchronizationHostedService> logger, MarketMetadataCollector metadataCollector) {
			_logger            = logger;
			_metadataCollector = metadataCollector;
			var ct = _cancellationTokenSource.Token;
			_timer = new Timer(_ => Task.Run(() => Sync(ct)), null, _interval, _interval);
		}

		public Task StartAsync(CancellationToken ct) {
			_timer.Change(TimeSpan.Zero, _interval);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken ct) {
			_cancellationTokenSource.Cancel();
			return Task.CompletedTask;
		}

		async Task Sync(CancellationToken ct) {
			_logger.LogInformation("Synchronization started");
			try {
				await _metadataCollector.Collect(ct);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
			}
			_logger.LogInformation("Synchronization finished");
		}
	}
}