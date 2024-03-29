using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Repository;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace InvestmentReporting.StateService {
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
			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "InvestmentReporting.StateService", Version = "v1" }));
			services.AddSharedAuthentication();
			services.AddMemoryCache();

			services.AddSingleton<IMongoDatabase>(_ => {
				var mongoSettings = MongoClientSettings.FromConnectionString(MongoConnectionString.Create());
				var client        = new MongoClient(mongoSettings);
				return client.GetDatabase("InvestmentReporting");
			});
			services.AddSingleton<IIdGenerator, ObjectIdGenerator>();
			services.AddSingleton<IStateRepository, MongoStateRepository>();
			services.AddSingleton<CurrencyConfiguration>();
			services.AddSingleton<StateManager>();
			services.AddSingleton<IStateManager>(sp => sp.GetRequiredService<StateManager>());
			services.AddSingleton<ReadStateUseCase>();
			services.AddSingleton<ResetStateUseCase>();
			services.AddSingleton<ReadOperationsUseCase>();
			services.AddSingleton<ResetOperationsUseCase>();
			services.AddSingleton<CreateBrokerUseCase>();
			services.AddSingleton<CreateAccountUseCase>();
			services.AddSingleton<AddIncomeUseCase>();
			services.AddSingleton<AddExpenseUseCase>();
			services.AddSingleton<BuyAssetUseCase>();
			services.AddSingleton<SellAssetUseCase>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if ( env.IsDevelopment() ) {
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvestmentReporting.StateService v1"));
			}

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}
	}
}