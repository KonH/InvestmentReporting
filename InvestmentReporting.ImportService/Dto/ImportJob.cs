using System;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.ImportService.Dto {
	public record ImportJob(DateTimeOffset Date, UserId User, BrokerId Broker, string Importer, byte[] Report, bool Completed = false, string Error = "");
}