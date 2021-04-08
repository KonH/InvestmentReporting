using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Import.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.ImportService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class ImportController : ControllerBase {
		readonly ILogger              _logger;
		readonly ImportUseCaseFactory _useCaseFactory;

		public ImportController(ILogger<ImportController> logger, ImportUseCaseFactory useCaseFactory) {
			_logger         = logger;
			_useCaseFactory = useCaseFactory;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post(
			[Required] DateTimeOffset date, [Required] string broker, [Required] string importer, [Required] IFormFile report) {
			_logger.LogInformation($"Import '{importer}' for broker '{broker}' started");
			var useCase = _useCaseFactory.Create(importer);
			await using var stream  = report.OpenReadStream();
			var userId  = new UserId(User.Identity?.Name ?? string.Empty);
			await useCase.Handle(date, userId, new(broker), stream);
			_logger.LogInformation($"Import '{importer}' for broker '{broker}' finished");
			return Ok();
		}
	}
}