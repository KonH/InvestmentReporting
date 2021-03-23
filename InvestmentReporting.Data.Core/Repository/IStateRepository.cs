using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;

namespace InvestmentReporting.Data.Core.Repository {
	public interface IStateRepository {
		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(DateTimeOffset date, string userId);

		public Task SaveCommand(ICommandModel model);
	}
}