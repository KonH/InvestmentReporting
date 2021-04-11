using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Domain.Command;
using InvestmentReporting.Domain.Entity;

namespace InvestmentReporting.Domain.Logic {
	public interface IStateManager {
		public Task<IReadOnlyDictionary<UserId, ReadOnlyState>> ReadStates(DateTimeOffset date);

		public Task<ReadOnlyState> ReadState(DateTimeOffset date, UserId id);

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, UserId id);

		public Task AddCommand(ICommand command);
	}
}