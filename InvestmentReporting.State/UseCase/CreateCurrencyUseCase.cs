using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase.Exceptions;

namespace InvestmentReporting.State.UseCase {
	public sealed class CreateCurrencyUseCase {
		readonly IStateManager _stateManager;
		readonly IIdGenerator  _idGenerator;

		public CreateCurrencyUseCase(IStateManager stateManager, IIdGenerator idGenerator) {
			_stateManager = stateManager;
			_idGenerator  = idGenerator;
		}

		public async Task Handle(DateTimeOffset date, UserId user, CurrencyCode code, CurrencyFormat format) {
			if ( string.IsNullOrWhiteSpace(code.ToString()) ) {
				throw new InvalidCurrencyException();
			}
			var formatStr = format.ToString();
			if ( !formatStr.Contains("{sign}") && !formatStr.Contains("{value}") ) {
				throw new InvalidCurrencyException();
			}
			var state = _stateManager.ReadState(date, user);
			if ( state.Currencies.Any(c => c.Code == code) ) {
				throw new DuplicateCurrencyException();
			}
			var id = new CurrencyId(_idGenerator.GenerateNewId());
			await _stateManager.AddCommand(new CreateCurrencyCommand(date, user, id, code, format));
		}
	}
}