using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.ImportService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.ImportService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class ImportController : ControllerBase {
		readonly ILogger                 _logger;
		readonly BackgroundImportService _service;

		public ImportController(ILogger<ImportController> logger, BackgroundImportService service) {
			_logger  = logger;
			_service = service;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post(
			[Required] DateTimeOffset date, [Required] string broker, [Required] string importer, [Required] IFormFile report) {
			_logger.LogInformation($"Import '{importer}' for broker '{broker}' started");
			await using var stream       = report.OpenReadStream();
			var             userId       = new UserId(User.Identity?.Name ?? string.Empty);
			var             memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream);
			var reportBytes = memoryStream.ToArray();
			_service.Schedule(new(date, userId, new(broker), importer, reportBytes));
			_logger.LogInformation($"Import '{importer}' for broker '{broker}' scheduled");
			return Ok();
		}
	}
}