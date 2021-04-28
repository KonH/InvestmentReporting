using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.AuthService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class LoginController : ControllerBase {
		static readonly TimeSpan _maxRandomDelay = TimeSpan.FromSeconds(0.25);

		readonly ILogger             _logger;
		readonly UserManager<User>   _userManager;
		readonly SignInManager<User> _signInManager;
		readonly Random              _random;

		public LoginController(ILogger<LoginController> logger, UserManager<User> userManager, SignInManager<User> signInManager) {
			_logger        = logger;
			_userManager   = userManager;
			_signInManager = signInManager;
			_random        = new Random();
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Login([Required] string userName, [Required] string password) {
			_logger.LogInformation($"{nameof(Login)}: user: '{userName}'");
			var user = await _userManager.FindByNameAsync(userName);
			_logger.LogInformation($"{nameof(Login)}: found user '{user?.Id}'");
			if ( user == null ) {
				await SafetyDelay();
				return BadRequest();
			}
			var result = await _signInManager.PasswordSignInAsync(user, password, true, false);
			if ( !result.Succeeded ) {
				_logger.LogWarning($"{nameof(Login)}: failed login: {result}");
				await SafetyDelay();
				return BadRequest();
			}
			_logger.LogInformation($"{nameof(Login)}: login success for '{user.Id}'");
			return Ok();
		}

		async Task SafetyDelay() =>
			await Task.Delay(_random.Next(_maxRandomDelay.Milliseconds));
	}
}