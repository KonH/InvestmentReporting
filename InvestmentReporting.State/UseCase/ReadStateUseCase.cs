using System;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class ReadStateUseCase {
		readonly IStateManager _stateManager;

		public ReadStateUseCase(IStateManager stateManager) {
			_stateManager = stateManager;
		}

		public ReadOnlyState Handle(DateTimeOffset date, UserId user) {
			if ( string.IsNullOrWhiteSpace(user.Value) ) {
				throw new InvalidUserException();
			}
			return _stateManager.ReadState(date, user);
		}
	}
}