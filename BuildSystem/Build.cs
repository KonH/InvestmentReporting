using System;
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
		static readonly (string, string)[] DotNetProjects = {
			("InvestmentReporting.AuthService", "auth.ts"),
			("InvestmentReporting.InviteService", "invite.ts"),
			("InvestmentReporting.StateService", "state.ts"),
			("InvestmentReporting.ImportService", "import.ts")
		};

		[Parameter]
		string Configuration = "Development";

		public static int Main() => Execute<Build>(x => x.Compile);

		Target Restore => _ => _
			.Executes(() =>
			{
				Run("Install api generator tool",
					RootDirectory,
					"dotnet", "tool install --global Swashbuckle.AspNetCore.Cli --version 5.4.1",
					ignoreExitCode: true);
				foreach ( var (project, _) in DotNetProjects ) {
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
				foreach ( var (project, _) in DotNetProjects ) {
					DotNetBuild(s => s
						.SetProjectFile(RootDirectory / project)
						.SetConfiguration(Configuration));
				}

				var shouldUseSwagger = (Configuration == "Development") && (architecture != Architecture.Arm64);
				if ( shouldUseSwagger ) {
					Environment.SetEnvironmentVariable("SWAGGER_RUN", true.ToString());
					try {
						var apiDir = RootDirectory / "api";
						EnsureExistingDirectory(apiDir);
						foreach ( var (project, api) in DotNetProjects ) {
							var swaggerPath = apiDir / $"{project}.swagger.json";
							var dllPath     = RootDirectory / project / "bin" / Configuration / "net5.0" / $"{project}.dll";
							Run("Generate swagger api file",
								RootDirectory / project,
								"swagger", $"tofile --output {swaggerPath} {dllPath} v1");
							Run("Generate api client from swagger file",
								RootDirectory / "Frontend",
								"npm", $"run generate-api -- --path ../api/{project}.swagger.json --output src/api --name \"{api}\"");
						}
					} finally {
						Environment.SetEnvironmentVariable("SWAGGER_RUN", false.ToString());
					}
				}

				foreach ( var (project, _) in DotNetProjects ) {
					DotNetPublish(s => s
						.SetProject(RootDirectory / project)
						.SetConfiguration(Configuration)
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

		Target Start => _ => _
			.DependsOn(Compile)
			.Executes(() =>
			{
				Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Configuration);
				Run("Running containers",
					RootDirectory,
					"docker-compose", $"--env-file {RootDirectory}/{Configuration}.env up -d");
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
