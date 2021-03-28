using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;

namespace InvestmentReporting.Data.InMemory.Repository {
	public sealed class InMemoryStateRepository : IStateRepository {
		readonly List<ICommandModel> _commands;

		public InMemoryStateRepository(List<ICommandModel> commands) {
			_commands = commands;
		}

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, string userId) =>
			Task.FromResult((IReadOnlyCollection<ICommandModel>)_commands
				.Where(c => c.Date >= startDate)
				.Where(c => c.Date <= endDate)
				.Where(c => c.User == userId)
				.ToArray());

		public Task SaveCommand(ICommandModel model) {
			_commands.Add(model);
			return Task.CompletedTask;
		}
	}
}