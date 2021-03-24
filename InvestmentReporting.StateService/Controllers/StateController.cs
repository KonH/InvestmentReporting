using InvestmentReporting.StateService.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class StateController : ControllerBase {
		readonly ILogger _logger;

		public StateController(ILogger<StateController> logger) {
			_logger = logger;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(StateDto), StatusCodes.Status200OK)]
		public IActionResult Get() {
			return new JsonResult(new StateDto(new [] {
				new BrokerDto("#1 DisplayName"),
				new BrokerDto("#2 DisplayName"),
			}));
		}
	}
}