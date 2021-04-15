using System.Threading;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
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
		readonly ILogger                _logger;
		readonly SynchronizationUseCase _useCase;

		public SyncController(ILogger<SyncController> logger, SynchronizationUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Post() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Request sync from user '{userId}'");
			await _useCase.Handle(CancellationToken.None);
			return Ok();
		}
	}
}