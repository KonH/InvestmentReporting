using System;
using System.IO;
using System.Threading.Tasks;
using AspNetCore.Identity.Mongo;
using InvestmentReporting.Shared.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace InvestmentReporting.Shared.Extensions {
	public static class AuthenticationExtensions {
		public static void AddSharedAuthentication(this IServiceCollection services) {
			if ( bool.TryParse(Environment.GetEnvironmentVariable("SWAGGER_RUN"), out var isSwaggerRun) && isSwaggerRun ) {
				// Workaround for invalid connection string issue while running swagger generation
				return;
			}
			var mongoUserName    = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_USERNAME");
			var mongoPassword    = Environment.GetEnvironmentVariable("MONGO_INITDB_ROOT_PASSWORD");
			var connectionString = $"mongodb://{mongoUserName}:{mongoPassword}@mongo:27017/InvestmentReporting?authSource=admin";
			services
				.AddIdentityMongoDbProvider<User, Role, ObjectId>(
					_ => {},
					mongoIdentityOpts => mongoIdentityOpts.ConnectionString = connectionString)
				.AddSignInManager();
			services.AddDataProtection()
				.PersistKeysToFileSystem(new DirectoryInfo("key_ring_storage"))
				.SetApplicationName("InvestmentReporting-Server");
			services.AddAuthentication();
			services.ConfigureApplicationCookie(opts => {
				opts.Cookie.Name = ".AspNet.SharedCookie";
				opts.Cookie.Path = "/";
				opts.LoginPath   = string.Empty;
				opts.LogoutPath  = string.Empty;
				opts.Events.OnRedirectToLogin = ctx => {
					ctx.RedirectUri         = string.Empty;
					ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return Task.CompletedTask;
				};
			});
		}
	}
}