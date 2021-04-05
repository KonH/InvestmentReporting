using System;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase.Exceptions;

namespace InvestmentReporting.Domain.UseCase {
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
			if ( !format.ToString().Contains("{0}") ) {
				throw new InvalidCurrencyException();
			}
			var state = await _stateManager.ReadState(date, user);
			if ( state.Currencies.Any(c => c.Code == code) ) {
				throw new DuplicateCurrencyException();
			}
			var id = new CurrencyId(_idGenerator.GenerateNewId());
			await _stateManager.AddCommand(new CreateCurrencyCommand(date, user, id, code, format));
		}
	}
}