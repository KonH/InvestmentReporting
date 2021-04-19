using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Meta.Dto;
using InvestmentReporting.Meta.Entity;
using InvestmentReporting.Meta.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MetaService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public sealed class DashboardConfigController : ControllerBase {
		readonly ILogger                      _logger;
		readonly ReadDashboardConfigsUseCase  _readUseCase;
		readonly UpdateDashboardConfigUseCase _updateUseCase;
		readonly RemoveDashboardConfigUseCase _removeUseCase;

		public DashboardConfigController(
			ILogger<DashboardConfigController> logger, ReadDashboardConfigsUseCase readUseCase,
			UpdateDashboardConfigUseCase updateUseCase, RemoveDashboardConfigUseCase removeUseCase) {
			_logger        = logger;
			_readUseCase   = readUseCase;
			_updateUseCase = updateUseCase;
			_removeUseCase = removeUseCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(DashboardConfigStateDto), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve dashboards for user '{userId}'");
			var state = await _readUseCase.Handle(userId);
			var dto   = new DashboardConfigStateDto(
				state.Dashboards
					.Select(d => new DashboardConfigDto(
						d.Id, d.Name, d.Tags
							.Select(t => new DashboardConfigTagDto(t.Tag, t.Target))
							.ToArray()))
					.ToArray());
			return new JsonResult(dto);
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Update([Required] DashboardConfigDto dashboard) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Update dashboard for user '{userId}': {dashboard}");
			var dashboardEntity = new DashboardConfig(
				new(dashboard.Id), dashboard.Name,
				dashboard.Tags
					.Select(t => new DashboardConfigTag(new(t.Tag), t.Target))
					.ToArray());
			await _updateUseCase.Handle(userId, dashboardEntity);
			return Ok();
		}

		[HttpDelete]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Delete([Required] string dashboard) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Remove dashboard '{dashboard}' for user '{userId}'");
			await _removeUseCase.Handle(userId, new(dashboard));
			return Ok();
		}
	}
}