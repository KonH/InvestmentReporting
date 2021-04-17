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
	public class CurrencyController : ControllerBase {
		readonly ILogger               _logger;
		readonly CreateCurrencyUseCase _useCase;

		public CurrencyController(ILogger<CurrencyController> logger, CreateCurrencyUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post([Required] string code, [Required] string format) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Creating currency '{code}' with format '{format}' for user '{userId}'");
			try {
				await _useCase.Handle(DateTimeOffset.MinValue, userId, new(code), new(format));
				return StatusCode(StatusCodes.Status201Created);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
				return BadRequest();
			}
		}
	}
}