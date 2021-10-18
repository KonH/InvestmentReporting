using System.Threading.Tasks;
using InvestmentReporting.ImportService.Dto;
using InvestmentReporting.State.Entity;
using InvestmentReporting.ImportService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentReporting.ImportService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class ImportStatusController : ControllerBase {
		readonly BackgroundImportService _service;

		public ImportStatusController(BackgroundImportService service) {
			_service = service;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(ImportStatus), StatusCodes.Status200OK)]
		public Task<IActionResult> Get() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			var job    = _service.GetJob(userId);
			var status = new ImportStatus(userId, job?.Completed ?? false, job?.Error ?? string.Empty);
			return Task.FromResult<IActionResult>(new JsonResult(status));
		}
	}
}