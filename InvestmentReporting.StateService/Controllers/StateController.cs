using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
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
		readonly ILogger           _logger;
		readonly ReadStateUseCase  _readUseCase;
		readonly ResetStateUseCase _resetUseCase;

		public StateController(
			ILogger<StateController> logger, ReadStateUseCase readUseCase, ResetStateUseCase resetUseCase) {
			_logger       = logger;
			_readUseCase  = readUseCase;
			_resetUseCase = resetUseCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(StateDto), StatusCodes.Status200OK)]
		public IActionResult Get([Required] DateTimeOffset date) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Retrieve state for user '{userId}' at {date}");
			var state  = _readUseCase.Handle(date, userId);
			var brokers = state.Brokers
				.Select(b => new BrokerDto(
					b.Id,
					b.DisplayName,
					b.Accounts
						.Select(a => new AccountDto(a.Id, a.Currency, a.DisplayName, a.Balance))
						.ToArray(),
					b.Inventory
						.Select(a => new AssetDto(a.Id, a.Isin, a.Count))
						.ToArray()))
				.ToArray();
			var currencies = state.Currencies
				.Select(c => new CurrencyDto(c.Id, c.Code, c.Format))
				.ToArray();
			var dto = new StateDto(brokers, currencies);
			return new JsonResult(dto);
		}

		[HttpDelete]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Delete() {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation($"Reset state for user '{userId}'");
			await _resetUseCase.Handle(userId);
			return Ok();
		}
	}
}