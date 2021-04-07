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
		readonly ILogger                      _logger;
		readonly ReadAccountOperationsUseCase _readAccountUseCase;
		readonly ReadAssetOperationsUseCase   _readAssetUseCase;
		readonly ResetOperationsUseCase       _resetUseCase;

		public OperationController(
			ILogger<OperationController> logger,
			ReadAccountOperationsUseCase readAccountUseCase, ReadAssetOperationsUseCase readAssetUseCase,
			ResetOperationsUseCase resetUseCase) {
			_logger             = logger;
			_readAccountUseCase = readAccountUseCase;
			_readAssetUseCase   = readAssetUseCase;
			_resetUseCase       = resetUseCase;
		}

		[HttpGet("ForAccount")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(OperationDto[]), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetForAccount(
			[Required] DateTimeOffset startDate, [Required] DateTimeOffset endDate, [Required] string broker, [Required] string account) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve operations for user '{userId}', broker '{broker}', account '{account}', at {startDate}-{endDate}");
			var operations = await _readAccountUseCase.Handle(startDate, endDate, userId, new(broker), new(account));
			var dto        = operations
				.Select(op => {
					var asset = (op.Asset != null) ? op.Asset.ToString() : null;
					return new OperationDto(op.Date, op.Kind.ToString(), op.Currency, op.Amount, op.Category, asset);
				});
			return new JsonResult(dto);
		}

		[HttpGet("ForAsset")]
		[Produces("application/json")]
		[ProducesResponseType(typeof(OperationDto[]), StatusCodes.Status200OK)]
		public async Task<IActionResult> GetForAsset(
			[Required] DateTimeOffset startDate, [Required] DateTimeOffset endDate, [Required] string broker, [Required] string asset) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve operations for user '{userId}', broker '{broker}', asset '{asset}', at {startDate}-{endDate}");
			var operations = await _readAssetUseCase.Handle(startDate, endDate, userId, new(broker), new(asset));
			var dto = operations
				.Select(op => new OperationDto(op.Date, op.Kind.ToString(), op.Currency, op.Amount, op.Category, null));
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