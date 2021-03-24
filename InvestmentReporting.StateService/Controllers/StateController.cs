using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.UseCase;
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
		readonly ILogger          _logger;
		readonly ReadStateUseCase _useCase;

		public StateController(ILogger<StateController> logger, ReadStateUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(StateDto), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			var state  = await _useCase.Handle(DateTimeOffset.MinValue, userId);
			var dto    = new StateDto(state.Brokers.Select(b => new BrokerDto(b.DisplayName)).ToArray());
			return new JsonResult(dto);
		}
	}
}