using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InvestmentReporting.Import.UseCase;
using InvestmentReporting.ImportService.Dto;
using InvestmentReporting.State.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvestmentReporting.ImportService.Services {
	public class BackgroundImportService {
		readonly ILogger              _logger;
		readonly IServiceProvider     _serviceProvider;

		readonly ConcurrentDictionary<UserId, ImportJob> _jobs = new();

		public BackgroundImportService(ILogger<BackgroundImportService> logger, IServiceProvider serviceProvider) {
			_logger          = logger;
			_serviceProvider = serviceProvider;
		}

		public ImportJob? GetJob(UserId user) => _jobs.GetValueOrDefault(user);

		public void Schedule(ImportJob dto) {
			var key = dto.User;
			if ( _jobs.TryGetValue(key, out var value) && (!value.Completed && string.IsNullOrEmpty(value.Error)) ) {
				throw new InvalidOperationException("Wait until current import was finished");
			}
			_jobs.AddOrUpdate(key, _ => dto, (_, _) => dto);
			_logger.LogInformation($"Scheduled as '{key}'");
			Task.Run(() => Import(key));
		}

		async Task Import(UserId key) {
			_logger.LogInformation($"Start importing '{key}'");
			var             dto      = _jobs[key];
			var             importer = dto.Importer;
			var             date     = dto.Date;
			var             userId   = dto.User;
			var             broker   = dto.Broker;
			await using var stream   = new MemoryStream(dto.Report);
			ImportJob       result;
			try {
				using var scope          = _serviceProvider.CreateScope();
				var       useCaseFactory = scope.ServiceProvider.GetRequiredService<ImportUseCaseFactory>();
				var       useCase        = useCaseFactory.Create(importer);
				await useCase.Handle(date, userId, broker, stream);
				result = dto with {
					Completed = true
				};
				_logger.LogInformation($"Done importing '{key}'");
			} catch ( Exception e ) {
				result = dto with {
					Error = e.ToString()
				};
				_logger.LogInformation($"Failed importing '{key}': {e}");
			}
			_jobs.AddOrUpdate(key, _ => result, (_, _) => result);
		}
	}
}