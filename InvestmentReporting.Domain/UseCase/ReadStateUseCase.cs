using System;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
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