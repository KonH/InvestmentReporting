using System.Runtime.InteropServices;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tools.DotNet;
using static InvestmentReporting.BuildSystem.CustomProcessTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace InvestmentReporting.BuildSystem {
	[UnsetVisualStudioEnvironmentVariables]
	class Build : NukeBuild {
		[Parameter]
		string Configuration = "Debug";

		public static int Main() => Execute<Build>(x => x.Compile);

		Target Restore => _ => _
			.Executes(() =>
			{
				Run("Install api generator tool",
					RootDirectory,
					"dotnet", "tool install --global Swashbuckle.AspNetCore.Cli --version 5.4.1",
					ignoreExitCode: true);
				DotNetRestore(s => s.SetProjectFile(RootDirectory / "InvestmentReporting.AuthService"));
				DotNetRestore(s => s.SetProjectFile(RootDirectory / "InvestmentReporting.InviteService"));
				Run("Restoring frontend packages",
					RootDirectory / "Frontend",
					"npm", "install");
			});

		Target Compile => _ => _
			.DependsOn(Restore)
			.Requires(() => Configuration)
			.Executes(() =>
			{
				var architecture = RuntimeInformation.ProcessArchitecture;

				var dotNetPlatform = GetDotNetBuildArchitecture(architecture);
				DotNetBuild(s => s
					.SetProjectFile(RootDirectory / "InvestmentReporting.AuthService")
					.SetConfiguration(Configuration));
				DotNetBuild(s => s
					.SetProjectFile(RootDirectory / "InvestmentReporting.InviteService")
					.SetConfiguration(Configuration));

				/*var apiDir = RootDirectory / "Frontend" / "api";
				EnsureExistingDirectory(apiDir);
				var swaggerPath = apiDir / "InvestmentReporting.TestService.swagger.json";
				var dllPath     = RootDirectory / "InvestmentReporting.TestService" / "bin" / Configuration / "net5.0" / "InvestmentReporting.TestService.dll";
				Run("Generate swagger api file",
					RootDirectory / "InvestmentReporting.TestService",
					"swagger", $"tofile --output {swaggerPath} {dllPath} v1");*/

				DotNetPublish(s => s
					.SetProject(RootDirectory / "InvestmentReporting.AuthService")
					.SetConfiguration(Configuration)
					.SetRuntime($"linux-musl-{dotNetPlatform}")
					.EnablePublishSingleFile()
					.EnableSelfContained()
					.SetOutput(RootDirectory / "InvestmentReporting.AuthService" / "publish"));

				DotNetPublish(s => s
					.SetProject(RootDirectory / "InvestmentReporting.InviteService")
					.SetConfiguration(Configuration)
					.SetRuntime($"linux-musl-{dotNetPlatform}")
					.EnablePublishSingleFile()
					.EnableSelfContained()
					.SetOutput(RootDirectory / "InvestmentReporting.InviteService" / "publish"));

				Run("Building frontend",
					RootDirectory / "Frontend",
					"npm", "run build");
				Run("Run linter for frontend",
					RootDirectory / "Frontend",
					"npm", "run lint");

				var dotnetImageSuffix = GetDotnetImageSuffix(architecture);
				var mongoImage        = GetMongoDockerImage(architecture);
				Run("Build containers",
					RootDirectory,
					"docker-compose",
					"build " +
					$"--build-arg DOTNET_IMAGE_SUFFIX={dotnetImageSuffix} " +
					$"--build-arg MONGO_IMAGE={mongoImage} ");
			});

		Target Start => _ => _
			.DependsOn(Compile)
			.Executes(() =>
			{
				Run("Running containers",
					RootDirectory,
					"docker-compose", "up -d");
			});

		Target Stop => _ => _
			.Executes(() =>
			{
				Run("Stopping containers",
					RootDirectory,
					"docker-compose", "down");
			});

		string GetDotNetBuildArchitecture(Architecture architecture) =>
			architecture switch {
				Architecture.Arm64 => "arm64",
				_                  => "x64"
			};

		string GetDotnetImageSuffix(Architecture architecture) =>
			architecture switch {
				Architecture.Arm64 => "arm64v8",
				_                  => "amd64"
			};

		string GetMongoDockerImage(Architecture architecture) =>
			architecture switch {
				Architecture.Arm64 => "arm64v8/mongo:4.4.4-bionic",
				_                  => "mongo:4.4.4-bionic"
			};
	}
}
