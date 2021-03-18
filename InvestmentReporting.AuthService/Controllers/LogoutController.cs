using System.Threading.Tasks;
using InvestmentReporting.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Authorize]
	[Route("[controller]")]
	public class LogoutController : ControllerBase {
		readonly ILogger             _logger;
		readonly SignInManager<User> _signInManager;

		public LogoutController(ILogger<LogoutController> logger, SignInManager<User> signInManager) {
			_logger        = logger;
			_signInManager = signInManager;
		}

		[HttpPost]
		public async Task<IActionResult> Logout() {
			_logger.LogInformation($"{nameof(Logout)}");
			await _signInManager.SignOutAsync();
			_logger.LogInformation($"{nameof(Logout)}: logged out");
			return Ok();
		}
	}
}