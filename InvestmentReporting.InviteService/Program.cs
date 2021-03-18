using InvestmentReporting.InviteService.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvestmentReporting.InviteService {
	public class Program {
		public static void Main(string[] args) {
			var host = CreateHostBuilder(args).Build();
			host.Services.GetRequiredService<InviteTokenService>().Rotate();
			host.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
	}
}