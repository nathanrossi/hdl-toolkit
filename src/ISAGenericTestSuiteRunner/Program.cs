// Copyright 2011 Nathan Rossi - http://nathanrossi.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Xilinx.Simulation;
using HDLToolkit.Xilinx;
using HDLToolkit;
using HDLToolkit.Framework;
using HDLToolkit.Framework.Simulation;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ISAGenericTestSuiteRunner
{
	class Program
	{
		static void Main(string[] args)
		{
			Logger.Instance.VerbosityLevel = Logger.Verbosity.Debug;

			XilinxRepository repo = new XilinxRepository();
			repo.AddSearchPath(PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "EDK", "hw"));
			repo.AddSearchPath(@"C:\svn\uni-projects\uqrarg\hardware\Repo");

			ILibrary avrLibrary = repo.GetLibrary("avr_core_v1_00_a");
			string avrLibPath = repo.GetLibraryDefaultRootPath("avr_core_v1_00_a");
			string avrTestPath = PathHelper.Combine(avrLibPath, "test");
			string pregenPrjFile = PathHelper.Combine(avrTestPath, "simulation.prj");

			string workingDirectory = SystemHelper.GetTemporaryDirectory();
			string fileTest = PathHelper.Combine(avrTestPath, "example_asm_test.txt");
			string fileTemplate = PathHelper.Combine(avrTestPath, "avr_proc_exec_test_template.vhd");
			string fileTemplateBuilt = PathHelper.Combine(workingDirectory, "testbench.vhd");

			// Load test bench
			TestBench bench = TestBench.Load(fileTest);

			// Generate test bench vhdl
			File.WriteAllText(fileTemplateBuilt, TestBenchGenerator.GenerateTestBench(bench, workingDirectory, fileTemplate));

			// Manually generate the prj file
			string prjFile = File.ReadAllText(pregenPrjFile);
			string prjFileGen = PathHelper.Combine(workingDirectory, "prj.prj");
			prjFile = prjFile + Environment.NewLine + string.Format("vhdl avr_core_v1_00_a \"{0}\"", fileTemplateBuilt) + Environment.NewLine;
			File.WriteAllText(prjFileGen, prjFile);

			Logger.Instance.WriteVerbose("Building Simulation");
			// Build the isim exe
			FuseBuild.BuildResult result = FuseBuild.BuildProject(workingDirectory, prjFileGen, "avr_core_v1_00_a.avr_proc_exec_test");

			// Setup and start simulation
			Logger.Instance.WriteVerbose("Starting Simulation");
			ISimSimulator simulator = new ISimSimulator(workingDirectory, result.ExecutableFile);
			Processor proc = new Processor(simulator);

			// Start
			simulator.Start();

			// Run until the first instruction is next
			proc.RunToNextValidInstruction();

			Logger.Instance.WriteVerbose("Simulation Ready");

			while (true)
			{
				if (bench.IsTestComplete())
				{
					break;
				}

				bench.RunAssertions(proc.GetCurrentState());
				proc.RunCycle();
			}

			// Stop processes
			simulator.Kill();
			simulator.WaitForExit();

			// Clean up
			Directory.Delete(workingDirectory, true);

			Console.Write("{0}", Path.GetFileName(fileTest));

			PrintAssertionsState(bench);
		}

		private static void PrintAssertionsState(TestBench test)
		{
			Console.CursorLeft = Console.WindowWidth - 12;
			Console.Write(" [ ");
			if (test.failedAssertions > 0 || test.passedAssertions == 0)
			{
				using (new ConsoleColorScope(ConsoleColor.Red))
				{
					Console.Write("failed");
				}
			}
			else
			{
				using (new ConsoleColorScope(ConsoleColor.Green))
				{
					Console.Write("passed");
				}
			}
			Console.WriteLine(" ]");
		}
	}
}
