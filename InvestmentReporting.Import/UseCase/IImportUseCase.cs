using System;
using System.IO;
using System.Threading.Tasks;
using InvestmentReporting.State.Entity;

namespace InvestmentReporting.Import.UseCase {
	public interface IImportUseCase {
		Task Handle(DateTimeOffset date, UserId user, BrokerId brokerId, Stream stream);
	}
}