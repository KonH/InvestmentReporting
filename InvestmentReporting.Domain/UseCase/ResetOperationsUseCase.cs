using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ResetOperationsUseCase {
		readonly StateManager _stateManager;

		public ResetOperationsUseCase(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task Handle(UserId user) {
			if ( string.IsNullOrWhiteSpace(user.Value) ) {
				throw new InvalidUserException();
			}
			var commands = await _stateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, user);
			var filterCommands = commands
				.Where(c => !(c is CreateCurrencyModel) && !(c is CreateBrokerModel) && !(c is CreateAccountModel))
				.ToArray();
			await _stateManager.DeleteCommands(filterCommands);
		}
	}
}