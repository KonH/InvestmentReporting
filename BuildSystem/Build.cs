using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.DotNet;
using static InvestmentReporting.BuildSystem.CustomProcessTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace InvestmentReporting.BuildSystem {
	[UnsetVisualStudioEnvironmentVariables]
	class Build : NukeBuild {
		public static int Main() => Execute<Build>(x => x.Compile);

		Target Restore => _ => _
			.Executes(() =>
			{
				Run("Install api generator tool",
					RootDirectory,
					"dotnet", "tool install --global Swashbuckle.AspNetCore.Cli --version 5.4.1",
					ignoreExitCode: true);
				DotNetRestore(s => s.SetProjectFile(RootDirectory / "InvestmentReporting.TestService"));
				Run("Restoring frontend packages",
					RootDirectory / "Frontend",
					"npm", "install");
			});

		Target Compile => _ => _
			.DependsOn(Restore)
			.Executes(() =>
			{
				DotNetBuild(s => s.SetProjectFile(RootDirectory / "InvestmentReporting.TestService"));
				var apiDir = RootDirectory / "Frontend" / "api";
				EnsureExistingDirectory(apiDir);
				var swaggerPath = apiDir / "InvestmentReporting.TestService.swagger.json";
				var dllPath     = RootDirectory / "InvestmentReporting.TestService" / "bin" / "Debug" / "net5.0" / "InvestmentReporting.TestService.dll";
				Run("Generate swagger api file",
					RootDirectory / "InvestmentReporting.TestService",
					"swagger", $"tofile --output {swaggerPath} {dllPath} v1");
				DotNetPublish(s => s
					.SetProject(RootDirectory / "InvestmentReporting.TestService")
					.SetRuntime("linux-musl-x64")
					.EnablePublishSingleFile()
					.EnableSelfContained());
				Run("Building frontend",
					RootDirectory / "Frontend",
					"npm", "run build");
				Run("Run linter for frontend",
					RootDirectory / "Frontend",
					"npm", "run lint");
			});

		Target Start => _ => _
			.DependsOn(Compile)
			.Executes(() =>
			{
				Run("Building containers",
					RootDirectory,
					"docker-compose", "up -d --build");
			});

		Target Stop => _ => _
			.Executes(() =>
			{
				Run("Stopping containers",
					RootDirectory,
					"docker-compose", "down");
			});
	}
}