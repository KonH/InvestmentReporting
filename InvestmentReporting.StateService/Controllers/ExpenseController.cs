﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.UseCase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.StateService.Controllers {
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class ExpenseController : ControllerBase {
		readonly ILogger           _logger;
		readonly AddExpenseUseCase _useCase;

		public ExpenseController(ILogger<ExpenseController> logger, AddExpenseUseCase useCase) {
			_logger  = logger;
			_useCase = useCase;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Post(
			[Required] DateTimeOffset date, [Required] string broker, [Required] string account,
			[Required] decimal amount, [Required] string category, string? asset) {
			var userId = new UserId(User.Identity?.Name ?? string.Empty);
			_logger.LogInformation(
				$"Add expense (amount: {amount}, category: '{category}', asset: '{asset}') " +
				$"at '{date}' for user '{userId}' on broker '{broker}' and account '{account}'");
			try {
				await _useCase.Handle(
					date, userId, new(broker), new(account), amount, new(category),
					!string.IsNullOrEmpty(asset) ? new(asset) : null);
				return StatusCode(StatusCodes.Status201Created);
			} catch ( Exception e ) {
				_logger.LogError(e.ToString());
				return BadRequest();
			}
		}
	}
}