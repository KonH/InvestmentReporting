using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class AccountController : ControllerBase {
		readonly ILogger              _logger;
		readonly CreateAccountUseCase _useCase;

		public AccountController(ILogger<AccountController> logger, CreateAccountUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post(
			[Required] string broker, [Required] string currency, [Required] string displayName) {
			var userId     = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation(
				$"Creating account '{displayName}' for broker '{broker}' with currency '{currency}' for user '{userId}'");
			try {
				await _useCase.Handle(DateTimeOffset.MinValue, userId, new(broker), new(currency), displayName);
				return StatusCode(StatusCodes.Status201Created);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
				return BadRequest();
			}
		}
	}
}