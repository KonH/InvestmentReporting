using System;
using System.Collections.Generic;
using System.Linq;
using InvestmentReporting.State.Entity;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;

namespace InvestmentReporting.UnitTests {
	public static class StateManagerExtensions {
		public static IReadOnlyCollection<Operation> ReadAccountOperations(
			this IStateManager stateManager, DateTimeOffset date, UserId user, BrokerId broker, AccountId account) =>
			new ReadOperationsUseCase(stateManager).Handle(DateTimeOffset.MinValue, date, user)
				.Where(op => (op.Broker == broker) && (op.Account == account))
				.ToArray();
	}
}