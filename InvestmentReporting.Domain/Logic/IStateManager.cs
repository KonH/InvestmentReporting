using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Logic {
	public interface IStateManager {
		public IReadOnlyDictionary<UserId, ReadOnlyState> ReadStates(DateTimeOffset date);

		public ReadOnlyState ReadState(DateTimeOffset date, UserId id);

		public IReadOnlyCollection<ICommandModel> ReadCommands(DateTimeOffset startDate, DateTimeOffset endDate);

		public IReadOnlyCollection<ICommandModel> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId id);

		public Task AddCommand(ICommand command);
	}
}