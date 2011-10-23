using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Xilinx.Simulation;
using HDLToolkit.Xilinx;
using HDLToolkit;
using HDLToolkit.Framework;

namespace ISAGenericTestSuiteRunner
{
	class Program
	{
		static void Main(string[] args)
		{
			XilinxRepository repo = new XilinxRepository();
			repo.AddSearchPath(PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "EDK", "hw"));
			repo.AddSearchPath(@"C:\svn\uni-projects\uqrarg\hardware\Repo");

			ILibrary avrLibrary = repo.GetLibrary("avr_core_v1_00_a");

			Logger.Instance.VerbosityLevel = Logger.Verbosity.Debug;

			PrjFile project = new PrjFile(repo);
			project.AddAllInLibrary(avrLibrary);

			foreach (IModule m in project.Modules)
			{
				ReferenceHelper.GetVhdlModuleReferences(m);
			}

			string workingDirectory = SystemHelper.GetTemporaryDirectory();
			FuseBuild.BuildResult result = FuseBuild.BuildProject(workingDirectory, project, "avr_core_v1_00_a.avr_proc_exec_test");

			Logger.Instance.WriteVerbose(result.BuildLog);

			if (result.Built)
			{
				Console.WriteLine("Build Succeeded");

				ISimSimulator simulator = new ISimSimulator(workingDirectory, result.ExecutableFile);
				Console.WriteLine("Starting Simulator...");
				simulator.Start();
				Console.WriteLine("Started Simulator");

				Console.WriteLine("Current sim time = {0}", simulator.GetCurrentTime());
			}
		}
	}
}
