using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

namespace InvestmentReporting.BuildSystem {
	static class CustomProcessTasks {
		public static void Run(
			string description, AbsolutePath workingDirectory, string tool, string? arguments = null,
			bool ignoreExitCode = false) {
			Logger.Info(description);
			using var proc = ProcessTasks.StartProcess(tool, arguments, workingDirectory);
			if ( ignoreExitCode ) {
				proc.AssertWaitForExit();
			} else {
				proc.AssertZeroExitCode();
			}
		}
	}
}