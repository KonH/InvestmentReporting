using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.State.Command;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.State.Logic {
	public interface IStateManager {
		IReadOnlyDictionary<UserId, ReadOnlyState> ReadStates(DateTimeOffset date);

		ReadOnlyState ReadState(DateTimeOffset date, UserId user);

		IEnumerable<ICommand> ReadCommands(DateTimeOffset startDate, DateTimeOffset endDate);

		Task AddCommand(ICommand command);
	}
}