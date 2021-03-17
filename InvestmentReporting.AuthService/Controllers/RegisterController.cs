using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class RegisterController : ControllerBase {
		readonly ILogger           _logger;
		readonly UserManager<User> _userManager;

		public RegisterController(ILogger<RegisterController> logger, UserManager<User> userManager) {
			_logger      = logger;
			_userManager = userManager;
		}

		[HttpPost]
		public async Task<IActionResult> Register(string userName, string password) {
			_logger.LogInformation($"{nameof(Register)}: new user: '{userName}'");
			var user = new User {
				UserName = userName
			};
			var result = await _userManager.CreateAsync(user, password);
			if ( result.Succeeded ) {
				return Ok();
			}
			return BadRequest(string.Join("\n", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
		}
	}
}