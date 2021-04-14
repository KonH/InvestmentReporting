using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Logic {
	public interface IStateManager {
		IReadOnlyDictionary<UserId, ReadOnlyState> ReadStates(DateTimeOffset date);

		ReadOnlyState ReadState(DateTimeOffset date, UserId user);

		IEnumerable<ICommand> ReadCommands(DateTimeOffset startDate, DateTimeOffset endDate);

		Task AddCommand(ICommand command);
	}
}