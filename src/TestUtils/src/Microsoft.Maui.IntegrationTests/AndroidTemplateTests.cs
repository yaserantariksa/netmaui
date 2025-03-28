using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.IntegrationTests.Android;

namespace Microsoft.Maui.IntegrationTests
{
	public class AndroidTemplateTests : BaseBuildTest
	{
		Emulator TestAvd = new Emulator();
		string testPackage = "";

		[OneTimeSetUp]
		public void AndroidTemplateFxtSetUp()
		{
			if (TestEnvironment.IsMacOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
				TestAvd.Abi = "arm64-v8a";

			TestAvd.InstallAvd();
		}

		[SetUp]
		public void AndroidTemplateSetUp()
		{
			var emulatorLog = Path.Combine(TestDirectory, $"emulator-launch-{DateTime.UtcNow.ToFileTimeUtc()}.log");
			Assert.IsTrue(TestAvd.LaunchAndWaitForAvd(720, emulatorLog), "Failed to launch Test AVD.");
		}

		[OneTimeTearDown]
		public void AndroidTemplateFxtTearDown()
		{
			Adb.KillEmulator(TestAvd.Id);

			// adb.exe can lock certain files on windows, kill it after tests complete
			if (TestEnvironment.IsWindows)
			{
				Adb.Run("kill-server", deviceId: TestAvd.Id);
				foreach (var p in Process.GetProcessesByName("adb.exe"))
					p.Kill();
			}
		}

		[TearDown]
		public void AndroidTemplateTearDown()
		{
			Adb.UninstallPackage(testPackage);
		}


		[Test]
		[TestCase("maui", "net6.0", "Debug")]
		[TestCase("maui", "net6.0", "Release")]
		[TestCase("maui", "net7.0", "Debug")]
		[TestCase("maui", "net7.0", "Release")]
		[TestCase("maui-blazor", "net6.0", "Debug")]
		[TestCase("maui-blazor", "net6.0", "Release")]
		[TestCase("maui-blazor", "net7.0", "Debug")]
		[TestCase("maui-blazor", "net7.0", "Release")]
		public void RunOnAndroid(string id, string framework, string config)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			AddInstrumentation(projectDir);

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: "Install", framework: $"{framework}-android", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to install. Check test output/attachments for errors.");

			testPackage = $"com.companyname.{Path.GetFileName(projectDir).ToLowerInvariant()}";
			Assert.IsTrue(XHarness.RunAndroid(testPackage, Path.Combine(projectDir, "xh-results"), -1),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}

		void AddInstrumentation(string projectDir)
		{
			var androidDir = Path.Combine(projectDir, "Platforms", "Android");
			var instDestination = Path.Combine(androidDir, "Instrumentation.cs");
			FileUtilities.CreateFileFromResource("TemplateLaunchInstrumentation.cs", instDestination);
			Assert.True(File.Exists(instDestination), "Failed to create Instrumentation.cs");
			FileUtilities.ReplaceInFile(instDestination, "namespace mauitemplate", $"namespace {Path.GetFileName(projectDir)}");

			FileUtilities.ReplaceInFile(Path.Combine(androidDir, "MainActivity.cs"),
				"MainLauncher = true",
				"MainLauncher = true, Name = \"com.microsoft.mauitemplate.MainActivity\"");
		}

	}
}
