using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IStateRepository {
		public IReadOnlyCollection<string> ReadUsers(DateTimeOffset endDate);

		public IReadOnlyCollection<ICommandModel> ReadCommands(DateTimeOffset startDate, DateTimeOffset endDate);

		public Task DeleteCommands(IReadOnlyCollection<ICommandModel> commands);

		public Task SaveCommand(ICommandModel model);
	}
}