using System.Threading.Tasks;
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
			await _stateManager.ResetOperations(user);
		}
	}
}