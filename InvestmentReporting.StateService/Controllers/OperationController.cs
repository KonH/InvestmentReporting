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
		readonly ILogger                _logger;
		readonly ReadOperationsUseCase  _readUseCase;
		readonly ResetOperationsUseCase _resetUseCase;

		public OperationController(
			ILogger<OperationController> logger,
			ReadOperationsUseCase readUseCase, ResetOperationsUseCase resetUseCase) {
			_logger       = logger;
			_readUseCase  = readUseCase;
			_resetUseCase = resetUseCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(OperationDto[]), StatusCodes.Status200OK)]
		public IActionResult Get(
			[Required] DateTimeOffset startDate, [Required] DateTimeOffset endDate) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve operations for user '{userId}', at {startDate}-{endDate}");
			var operations = _readUseCase.Handle(startDate, endDate, userId);
			var dto        = operations
				.Select(op => {
					var asset = (op.Asset != null) ? op.Asset.ToString() : null;
					return new OperationDto(
						op.Date, op.Kind.ToString(), op.Currency, op.Amount, op.Category, op.Broker, op.Account, asset);
				});
			return new JsonResult(dto);
		}

		[HttpDelete]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Delete() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Reset operations for user '{userId}'");
			await _resetUseCase.Handle(userId);
			return Ok();
		}
	}
}