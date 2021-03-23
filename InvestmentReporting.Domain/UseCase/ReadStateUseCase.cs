using System;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
	public sealed class ReadStateUseCase {
		readonly StateManager _stateManager;

		public ReadStateUseCase(StateManager stateManager) {
			_stateManager = stateManager;
		}

		public async Task<ReadOnlyState> Handle(DateTimeOffset date, UserId user) {
			if ( string.IsNullOrWhiteSpace(user.Value) ) {
				throw new InvalidUserException();
			}
			return await _stateManager.Read(date, user);
		}
	}
}