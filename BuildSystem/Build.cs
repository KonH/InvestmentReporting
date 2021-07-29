using System;
using System.IO;
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
		static readonly (string, string)[] _dotNetProjects = {
			("InvestmentReporting.AuthService", "auth.ts"),
			("InvestmentReporting.InviteService", "invite.ts"),
			("InvestmentReporting.StateService", "state.ts"),
			("InvestmentReporting.ImportService", "import.ts"),
			("InvestmentReporting.MarketService", "market.ts"),
			("InvestmentReporting.MetaService", "meta.ts")
		};

		[Parameter]
		string Configuration = "Development";

		[Parameter]
		bool LegacyEnv = true;

		string DotNetConfiguration => (Configuration == "Production") ? "Release" : "Debug";

		public static int Main() => Execute<Build>(x => x.Compile);

		Target Restore => _ => _
			.Executes(() =>
			{
				foreach ( var (project, _) in _dotNetProjects ) {
					DotNetRestore(s => s.SetProjectFile(RootDirectory / project));
				}
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
				foreach ( var (project, _) in _dotNetProjects ) {
					DotNetBuild(s => s
						.SetProjectFile(RootDirectory / project)
						.SetConfiguration(DotNetConfiguration));
				}

				var shouldUseSwagger = (Configuration == "Development") && (architecture != Architecture.Arm64);
				if ( shouldUseSwagger ) {
					Environment.SetEnvironmentVariable("SWAGGER_RUN", true.ToString());
					try {
						var apiDir = RootDirectory / "api";
						EnsureExistingDirectory(apiDir);
						foreach ( var (project, api) in _dotNetProjects ) {
							var swaggerPath = apiDir / $"{project}.swagger.json";
							var dllPath     = RootDirectory / project / "bin" / DotNetConfiguration / "net5.0" / $"{project}.dll";
							Run("Generate swagger api file",
								RootDirectory / project,
								"dotnet", $"swagger tofile --output {swaggerPath} {dllPath} v1");
							Run("Generate api client from swagger file",
								RootDirectory / "Frontend",
								"npm", $"run generate-api -- --path ../api/{project}.swagger.json --output src/api --name \"{api}\"");
						}
					} finally {
						Environment.SetEnvironmentVariable("SWAGGER_RUN", false.ToString());
					}
				}

				foreach ( var (project, _) in _dotNetProjects ) {
					DotNetPublish(s => s
						.SetProject(RootDirectory / project)
						.SetConfiguration(DotNetConfiguration)
						.SetRuntime($"linux-musl-{dotNetPlatform}")
						.EnablePublishSingleFile()
						.EnableSelfContained()
						.SetOutput(RootDirectory / project / "publish"));
				}

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

		public Target Test => _ => _
			.DependsOn(Compile)
			.Executes(() =>
			{
				var architecture = RuntimeInformation.ProcessArchitecture;
				if ( architecture == Architecture.Arm64 ) {
					return;
				}
				DotNetTest(s => s.SetProjectFile("InvestmentReporting.UnitTests"));
			});

		public Target Start => _ => _
			.DependsOn(Compile)
			.DependsOn(Test)
			.Executes(() =>
			{
				Logger.Info("Define 'ASPNETCORE_ENVIRONMENT'");
				Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Configuration);
				var envFile = $"{RootDirectory}/{Configuration}.env";
				var command = "up -d";
				if ( LegacyEnv ) {
					Logger.Info($"Use env file '{envFile}' manually");
					var envLines = File.ReadAllLines(envFile);
					foreach ( var envLine in envLines ) {
						if ( string.IsNullOrEmpty(envLine) ) {
							continue;
						}
						var parts     = envLine.Split('=');
						var envName   = parts[0];
						var envValue  = parts[1];
						var showValue = Configuration == "Development";
						Logger.Info($"Define '{envName}': '{(showValue ? envValue : "hidden")}'");
						Environment.SetEnvironmentVariable(envName, envValue);
					}
				} else {
					command = $"--env-file {envFile} " + command;
				}
				Run("Running containers",
					RootDirectory,
					"docker-compose", command);
			});

		public Target Stop => _ => _
			.Executes(() =>
			{
				Run("Stopping containers",
					RootDirectory,
					"docker-compose", "stop");
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
