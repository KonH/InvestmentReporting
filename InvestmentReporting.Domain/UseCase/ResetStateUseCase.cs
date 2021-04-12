using System;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ResetStateUseCase {
		readonly StateManager _stateManager;

		public ResetStateUseCase(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task Handle(UserId user) {
			if ( string.IsNullOrWhiteSpace(user.Value) ) {
				throw new InvalidUserException();
			}
			var commands = _stateManager.ReadCommands(DateTimeOffset.MinValue, DateTimeOffset.MaxValue, user);
			await _stateManager.DeleteCommands(commands);
		}
	}
}