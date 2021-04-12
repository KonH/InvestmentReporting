using System;
using System.Threading;
using System.Threading.Tasks;
using InvestmentReporting.Market.UseCase;
using Microsoft.Extensions.Hosting;

namespace InvestmentReporting.MarketService.Services {
	public sealed class SynchronizationHostedService : IHostedService {
		readonly TimeSpan _interval = TimeSpan.FromDays(1);

		readonly CancellationTokenSource _cancellationTokenSource = new();

		readonly Timer _timer;

		public SynchronizationHostedService(SynchronizationUseCase useCase) {
			var ct = _cancellationTokenSource.Token;
			_timer = new Timer(_ => Task.Run(() => useCase.Sync(ct)), null, _interval, _interval);
		}

		public Task StartAsync(CancellationToken ct) {
			_timer.Change(TimeSpan.Zero, _interval);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken ct) {
			_cancellationTokenSource.Cancel();
			return Task.CompletedTask;
		}
	}
}