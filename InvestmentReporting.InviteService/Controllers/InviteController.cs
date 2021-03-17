using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.InviteService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class InviteController : ControllerBase {
		readonly ILogger _logger;

		public InviteController(ILogger<InviteController> logger) {
			_logger = logger;
		}

		[HttpPost("{token}")]
		public async Task<IActionResult> Invite(string token) {
			_logger.LogInformation($"{nameof(Invite)}: token: '{token}'");
			return Ok();
		}
	}
}