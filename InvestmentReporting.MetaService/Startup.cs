using InvestmentReporting.Data.Core.Repository;
using InvestmentReporting.Data.Mongo.Repository;
using InvestmentReporting.State.Logic;
using InvestmentReporting.State.UseCase;
using InvestmentReporting.Market.Logic;
using InvestmentReporting.Market.UseCase;
using InvestmentReporting.Meta.Logic;
using InvestmentReporting.Meta.UseCase;
using InvestmentReporting.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

namespace InvestmentReporting.MetaService {
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
			services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "InvestmentReporting.MetaService", Version = "v1" }));
			services.AddSharedAuthentication();
			services.AddMemoryCache();

			services.AddSingleton<IMongoDatabase>(_ => {
				var mongoSettings = MongoClientSettings.FromConnectionString(MongoConnectionString.Create());
				var client        = new MongoClient(mongoSettings);
				return client.GetDatabase("InvestmentReporting");
			});
			services.AddSingleton<IIdGenerator, ObjectIdGenerator>();
			services.AddSingleton<IStateRepository, MongoStateRepository>();
			services.AddSingleton<IStateManager, StateManager>();
			services.AddSingleton<IAssetTagRepository, MongoAssetTagRepository>();
			services.AddSingleton<IAssetMetadataRepository, MongoAssetMetadataRepository>();
			services.AddSingleton<ICurrencyPriceRepository, MongoCurrencyPriceRepository>();
			services.AddSingleton<IAssetPriceRepository, MongoAssetPriceRepository>();
			services.AddSingleton<IDashboardRepository, MongoDashboardRepository>();
			services.AddSingleton<CurrencyConfiguration>();
			services.AddSingleton<MetadataManager>();
			services.AddSingleton<AssetTagManager>();
			services.AddSingleton<DashboardManager>();
			services.AddSingleton<AssetPriceManager>();
			services.AddSingleton<CurrencyPriceManager>();
			services.AddSingleton<ExchangeManager>();
			services.AddSingleton<ReadStateUseCase>();
			services.AddSingleton<ReadVirtualStateUseCase>();
			services.AddSingleton<ReadAssetTagsUseCase>();
			services.AddSingleton<AddAssetTagUseCase>();
			services.AddSingleton<RemoveAssetTagUseCase>();
			services.AddSingleton<ReadDashboardConfigsUseCase>();
			services.AddSingleton<UpdateDashboardConfigUseCase>();
			services.AddSingleton<RemoveDashboardConfigUseCase>();
			services.AddSingleton<ReadDashboardStateUseCase>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
			if ( env.IsDevelopment() ) {
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvestmentReporting.MetaService v1"));
			}

			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}
	}
}