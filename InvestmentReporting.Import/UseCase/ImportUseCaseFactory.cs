using System;
using InvestmentReporting.Import.AlphaDirectMyBroker;
using InvestmentReporting.Import.TinkoffBrokerReport;

namespace InvestmentReporting.Import.UseCase {
	public sealed class ImportUseCaseFactory {
		readonly Func<Type, IImportUseCase> _factory;

		public ImportUseCaseFactory(Func<Type, IImportUseCase> factory) {
			_factory = factory;
		}

		public IImportUseCase Create(string importerName) =>
			importerName switch {
				"AlphaDirectMyBroker" => _factory(typeof(AlphaDirectImportUseCase)),
				"TinkoffBrokerReport" => _factory(typeof(TinkoffImportUseCase)),
				_                     => throw new NotSupportedException("Importer '{}' is unknown")
			};
	}
}