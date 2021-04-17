using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class BrokerController : ControllerBase {
		readonly ILogger             _logger;
		readonly CreateBrokerUseCase _useCase;

		public BrokerController(ILogger<BrokerController> logger, CreateBrokerUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post([Required] string displayName) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Creating broker '{displayName}' for user '{userId}'");
			try {
				await _useCase.Handle(DateTimeOffset.MinValue, userId, displayName);
				return StatusCode(StatusCodes.Status201Created);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
				return BadRequest();
			}
		}
	}
}