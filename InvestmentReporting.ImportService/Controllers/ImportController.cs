using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
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
		readonly ILogger       _logger;
		readonly ImportUseCase _useCase;

		public ImportController(ILogger<ImportController> logger, ImportUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post([Required] DateTimeOffset date, [Required] string broker, [Required] IFormFile report) {
			_logger.LogInformation($"Import for broker '{broker}' started");
			var xmlDoc = LoadXml(report);
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			await _useCase.Handle(date, userId, new(broker), xmlDoc);
			_logger.LogInformation($"Import for broker '{broker}' finished");
			return Ok();
		}

		static XmlDocument LoadXml(IFormFile file) {
			using var reader = new StreamReader(file.OpenReadStream());

			var content     = reader.ReadToEnd();
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(content);
			return xmlDocument;
		}
	}
}