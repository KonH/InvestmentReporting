using System.Threading;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Market.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MarketService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public sealed class SyncController : ControllerBase {
		readonly ILogger                  _logger;
		readonly SynchronizationUseCase   _syncUseCase;
		readonly MarketCandleResetUseCase _resetUseCase;

		public SyncController(
			ILogger<SyncController> logger, SynchronizationUseCase syncUseCase, MarketCandleResetUseCase resetUseCase) {
			_logger       = logger;
			_syncUseCase  = syncUseCase;
			_resetUseCase = resetUseCase;
		}

		[HttpPost("Sync")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Post() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Request sync from user '{userId}'");
			await _syncUseCase.Handle(CancellationToken.None);
			return Ok();
		}

		[HttpDelete("Reset")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Reset() {
			_logger.LogInformation("Request reset");
			await _resetUseCase.Handle();
			return Ok();
		}
	}
}