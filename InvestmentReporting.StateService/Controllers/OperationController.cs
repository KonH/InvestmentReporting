using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.StateService.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class OperationController : ControllerBase {
		readonly ILogger               _logger;
		readonly ReadOperationsUseCase _useCase;

		public OperationController(ILogger<OperationController> logger, ReadOperationsUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(OperationDto[]), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get(
			[Required] DateTimeOffset startDate, [Required] DateTimeOffset endDate, [Required] string broker, [Required] string account) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve operations for user '{userId}', broker '{broker}', account '{account}', at {startDate}-{endDate}");
			var operations = await _useCase.Handle(startDate, endDate, userId, new(broker), new(account));
			var dto        = operations
				.Select(op => new OperationDto(op.Date, op.Kind.ToString(), op.Amount, op.Category));
			return new JsonResult(dto);
		}
	}
}