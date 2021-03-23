using System;
using InvestmentReporting.Data.Core.Repository;

namespace InvestmentReporting.Data.InMemory.Repository {
	public sealed class GuidIdGenerator : IIdGenerator {
		public string GenerateNewId() => Guid.NewGuid().ToString();
	}
}