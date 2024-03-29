using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using InvestmentReporting.State.Entity;
using InvestmentReporting.Market.Dto;
using InvestmentReporting.Market.Entity;
using InvestmentReporting.Market.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.MarketService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public sealed class VirtualStateController : ControllerBase {
		readonly ILogger                 _logger;
		readonly ReadVirtualStateUseCase _useCase;

		public VirtualStateController(ILogger<VirtualStateController> logger, ReadVirtualStateUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(typeof(VirtualStateDto), StatusCodes.Status200OK)]
		public IActionResult Get([Required] DateTimeOffset date, string period, string broker) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation(
				$"Retrieve virtual state for user '{userId}' at {date} with period '{period}' and broker '{broker}'");
			var periodValue = !string.IsNullOrEmpty(period) ? Enum.Parse<VirtualPeriod>(period) : VirtualPeriod.AllTime;
			var brokerId    = !string.IsNullOrEmpty(broker) ? new BrokerId(broker) : null;
			var state       = _useCase.Handle(date, userId, periodValue, brokerId);
			var summary = state.Summary
				.ToDictionary(s => s.Key.ToString(), s => new CurrencyBalanceDto(s.Value.RealSum, s.Value.VirtualSum));
			var balancesDto = state.Balances
				.Select(b => {
					var inventoryDto = b.Inventory
						.Select(a => {
							var dividend = new DividendStateDto(
								a.Dividend.PreviousDividend, a.Dividend.LastDividend,
								a.Dividend.YearDividend, a.Dividend.DividendSum);
							return new VirtualAssetDto(
								a.Id, a.Broker, a.Isin,
								a.Name ?? string.Empty, a.Type ?? string.Empty,
								a.Count, a.RealPrice, a.VirtualPrice, a.RealSum, a.VirtualSum,
								dividend, a.Currency);
						})
						.ToArray();
					return new VirtualBalanceDto(b.RealSum, b.VirtualSum, inventoryDto, b.Currency);
				})
				.ToArray();
			var dto = new VirtualStateDto(summary, balancesDto);
			return new JsonResult(dto);
		}
	}
}