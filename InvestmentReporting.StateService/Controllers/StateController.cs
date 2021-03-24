using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class StateController : ControllerBase {
		readonly ILogger _logger;

		public StateController(ILogger<StateController> logger) {
			_logger = logger;
		}
	}
}