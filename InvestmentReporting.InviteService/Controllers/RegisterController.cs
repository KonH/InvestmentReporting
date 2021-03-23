using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.InviteService.Services;
using InvestmentReporting.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.InviteService.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class RegisterController : ControllerBase {
		readonly ILogger            _logger;
		readonly InviteTokenService _tokenService;
		readonly UserManager<User>  _userManager;

		public RegisterController(
			ILogger<RegisterController> logger, InviteTokenService tokenService, UserManager<User> userManager) {
			_logger       = logger;
			_tokenService = tokenService;
			_userManager  = userManager;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Register(
			[Required] string token, [Required] string userName, [Required] string password) {
			_logger.LogInformation($"{nameof(Register)}: token: '{token}', userName: '{userName}'");
			if ( token != _tokenService.ActiveToken ) {
				_logger.LogWarning($"{nameof(Register)}: invalid token provided");
				return BadRequest();
			}
			var user = new User {
				UserName = userName
			};
			var createResult = await _userManager.CreateAsync(user, password);
			if ( !createResult.Succeeded ) {
				_logger.LogWarning($"{nameof(Register)}: failed to create user: {createResult}");
				return BadRequest();
			}
			_logger.LogInformation($"{nameof(Register)}: user created '{userName}'");
			_tokenService.Rotate();
			return Ok();
		}
	}
}