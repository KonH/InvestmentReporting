using System;
using System.Collections.Generic;

namespace InvestmentReporting.Import.UseCase {
	public sealed class ImportUseCaseFactory {
		readonly Func<Type, IImportUseCase> _factory;

		readonly Dictionary<string, Type> _importers = new();

		public ImportUseCaseFactory(Func<Type, IImportUseCase> factory) {
			_factory = factory;
		}

		public void Register<T>(string name) where T : IImportUseCase {
			_importers.Add(name, typeof(T));
		}

		public IImportUseCase Create(string importerName) =>
			_importers.TryGetValue(importerName, out var importerType)
				? _factory(importerType)
				: throw new NotSupportedException($"Importer '{importerName}' is unknown");
	}
}