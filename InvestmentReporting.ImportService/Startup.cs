using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Repository;
using InvestmentReporting.Domain.Logic;
using InvestmentReporting.Domain.UseCase;
using InvestmentReporting.Import.AlphaDirectMyBroker;
using InvestmentReporting.Import.Logic;
using InvestmentReporting.Import.UseCase;
using InvestmentReporting.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace InvestmentReporting.ImportService {
	public class Startup {
		public Startup(IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services) {
			services.AddControllers()
				.ConfigureApiBehaviorOptions(opts => {
					opts.SuppressModelStateInvalidFilter = true;
					opts.SuppressMapClientErrors         = true;
				});
			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "InvestmentReporting.ImportService", Version = "v1" }));
			services.AddSharedAuthentication();
			services.AddMemoryCache();

			services.AddSingleton<IMongoDatabase>(_ => {
				var mongoSettings = MongoClientSettings.FromConnectionString(MongoConnectionString.Create());
				var client        = new MongoClient(mongoSettings);
				return client.GetDatabase("InvestmentReporting");
			});
			services.AddSingleton<IIdGenerator, ObjectIdGenerator>();
			services.AddSingleton<IStateRepository, MongoStateRepository>();
			services.AddSingleton<StateManager>();
			services.AddScoped<TransactionStateManager>();
			services.AddScoped<IStateManager>(sp => sp.GetRequiredService<TransactionStateManager>());
			services.AddScoped(sp => new ImportUseCaseFactory(t => (IImportUseCase)sp.GetRequiredService(t)));
			services.AddScoped<AlphaDirectImportUseCase>();
			services.AddScoped<BrokerMoneyMoveParser>();
			services.AddScoped<TradeParser>();
			services.AddScoped<AddIncomeUseCase>();
			services.AddScoped<AddExpenseUseCase>();
			services.AddScoped<BuyAssetUseCase>();
			services.AddScoped<SellAssetUseCase>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if ( env.IsDevelopment() ) {
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvestmentReporting.ImportService v1"));
			}

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}
	}
}