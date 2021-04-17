using System.Threading.Tasks;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
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