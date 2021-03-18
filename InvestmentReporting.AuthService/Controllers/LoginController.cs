using System.Threading.Tasks;
using InvestmentReporting.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class LoginController : ControllerBase {
		readonly ILogger             _logger;
		readonly UserManager<User>   _userManager;
		readonly SignInManager<User> _signInManager;

		public LoginController(ILogger<LoginController> logger, UserManager<User> userManager, SignInManager<User> signInManager) {
			_logger        = logger;
			_userManager   = userManager;
			_signInManager = signInManager;
		}

		[HttpPost]
		public async Task<IActionResult> Login(string userName, string password) {
			_logger.LogInformation($"{nameof(Login)}: user: '{userName}'");
			var user = await _userManager.FindByNameAsync(userName);
			_logger.LogInformation($"{nameof(Login)}: found user '{user?.Id}'");
			if ( user == null ) {
				return BadRequest();
			}
			var result = await _signInManager.PasswordSignInAsync(user, password, true, false);
			if ( !result.Succeeded ) {
				_logger.LogWarning($"{nameof(Login)}: failed login: {result}");
				return BadRequest();
			}
			_logger.LogInformation($"{nameof(Login)}: login success for '{user.Id}'");
			return Ok();
		}
	}
}