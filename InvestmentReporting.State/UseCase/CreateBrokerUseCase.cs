using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class CreateBrokerUseCase {
		readonly IStateManager _stateManager;
		readonly IIdGenerator  _idGenerator;

		public CreateBrokerUseCase(IStateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(DateTimeOffset date, UserId user, string displayName) {
			if ( string.IsNullOrWhiteSpace(displayName) ) {
				throw new InvalidBrokerException();
			}
			var state = _stateManager.ReadState(date, user);
			if ( state.Brokers.Any(b => b.DisplayName == displayName) ) {
				throw new DuplicateBrokerException();
			}
			var id = new BrokerId(_idGenerator.GenerateNewId());
			await _stateManager.AddCommand(new CreateBrokerCommand(date, user, id, displayName));
		}
	}
}