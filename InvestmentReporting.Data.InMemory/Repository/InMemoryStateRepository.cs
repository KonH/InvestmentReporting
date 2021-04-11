using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentReporting.Data.Core.Model;
using InvestmentReporting.Data.Core.Repository;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.Data.InMemory.Repository {
	public sealed class InMemoryStateRepository : IStateRepository {
		readonly ILogger             _logger;
		readonly List<ICommandModel> _commands;

		public InMemoryStateRepository(ILogger<InMemoryStateRepository> logger, List<ICommandModel> commands) {
			_logger   = logger;
			_commands = commands;
		}

		public Task<IReadOnlyCollection<string>> ReadUsers(DateTimeOffset endDate) =>
			Task.FromResult<IReadOnlyCollection<string>>(_commands
				.Select(c => c.User)
				.Distinct()
				.ToArray());

		public Task<IReadOnlyCollection<ICommandModel>> ReadCommands(
			DateTimeOffset startDate, DateTimeOffset endDate, string userId) =>
			Task.FromResult((IReadOnlyCollection<ICommandModel>)_commands
				.Where(c => c.Date >= startDate)
				.Where(c => c.Date <= endDate)
				.Where(c => c.User == userId)
				.ToArray());

		public Task SaveCommand(ICommandModel model) {
			_commands.Add(model);
			_logger.LogTrace($"Command saved: {model}");
			return Task.CompletedTask;
		}

		public Task DeleteCommands(IReadOnlyCollection<ICommandModel> commands) {
			_commands.RemoveAll(commands.Contains);
			return Task.CompletedTask;
		}
	}
}