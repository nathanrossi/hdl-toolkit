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
			XilinxRepository repo = new XilinxRepository();
			repo.AddSearchPath(PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "EDK", "hw"));
			repo.AddSearchPath(@"C:\svn\uni-projects\uqrarg\hardware\Repo");

			ILibrary avrLibrary = repo.GetLibrary("avr_core_v1_00_a");
			string avrLibPath = repo.GetLibraryDefaultRootPath("avr_core_v1_00_a");
			string avrTestPath = PathHelper.Combine(avrLibPath, "test");
			string pregenPrjFile = PathHelper.Combine(avrTestPath, "simulation.prj");

			Logger.Instance.VerbosityLevel = Logger.Verbosity.Debug;

			/*PrjFile project = new PrjFile(repo);
			project.AddAllInLibrary(avrLibrary);

			foreach (IModule m in project.Modules)
			{
				ReferenceHelper.GetVhdlModuleReferences(m);
			}*/

			string workingDirectory = SystemHelper.GetTemporaryDirectory();
			string fileTest = PathHelper.Combine(avrTestPath, "example_asm_test.txt");
			string fileTemplate = PathHelper.Combine(avrTestPath, "avr_proc_exec_test_template.vhd");
			string fileTemplateBuilt = PathHelper.Combine(workingDirectory, "testbench.vhd");

			Console.WriteLine("Generated Raw Assembly File");

			TestBench bench = TestBench.Load(fileTest);
			File.WriteAllText(PathHelper.Combine(workingDirectory, "assembly.s"), bench.GenerateAssembly());

			MemoryStream machineCode = GenerateMachineCode(workingDirectory, PathHelper.Combine(workingDirectory, "assembly.s"));
			File.WriteAllText(fileTemplateBuilt, TestBenchTemplate(machineCode, fileTemplate));

			Console.WriteLine("Generated TestBench File Succeeded");

			Console.WriteLine("Waiting...");
			Console.ReadLine();

			Console.WriteLine("Generate Prj File");

			string prjFile = File.ReadAllText(pregenPrjFile);
			string prjFileGen = PathHelper.Combine(workingDirectory, "prj.prj");
			prjFile = prjFile + Environment.NewLine + string.Format("vhdl avr_core_v1_00_a \"{0}\"", fileTemplateBuilt) + Environment.NewLine;
			File.WriteAllText(prjFileGen, prjFile);

			Console.WriteLine("Generated Prj File");

			Console.WriteLine("Building...");
			Console.ReadLine();

			FuseBuild.BuildResult result = FuseBuild.BuildProject(workingDirectory, prjFileGen, "avr_core_v1_00_a.avr_proc_exec_test");

			Console.WriteLine("Build Succeeded");

			ISimSimulator simulator = new ISimSimulator(workingDirectory, result.ExecutableFile);
			Console.WriteLine("Starting Simulator...");
			simulator.Start();
			Console.WriteLine("Started Simulator");

			ProcessorState state = new ProcessorState(simulator);
			simulator.RunFor(10);
			while (!state.IsNextPCValid())
			{
				simulator.RunFor(10);
			}

			Console.WriteLine("Pipeline Full, Next instruction is 0x0000");

			while (true)
			{
				//Console.WriteLine("State = {0}", state.IsPipelineStalled() ? "stalled" : "executing");
				//Console.WriteLine("PC: 0x{0:X4} (Valid = {1})", state.GetPC(), state.IsPCValid());
				//Console.WriteLine("SREG: 0x{0:X2}", state.GetStatusRegister());

				//PrintRegisterFile(state);

				//if (Console.ReadLine().Contains("x"))
				//{
					//break;
				//}

				if (bench.ProcessTestBench(state))
				{
					simulator.RunFor(10);
				}
				else
				{
					break;
				}
			}

			simulator.Kill();
			simulator.WaitForExit();

			Directory.Delete(workingDirectory, true);

			Console.Write("Test Bench [ ");
			if (bench.failedAssertions > 0 || bench.passedAssertions == 0)
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
			Console.ReadLine();
		}

		public class ProcessorState
		{
			public ISimSimulator Simulator { get; set; }

			public ProcessorState(ISimSimulator sim)
			{
				Simulator = sim;
			}

			public int GetPC()
			{
				return (int)(Simulator.GetSignalState("UUT/pcs(1)").Flip().ToLong());
			}

			public int GetNextPC()
			{
				return (int)(Simulator.GetSignalState("UUT/pcs(0)").Flip().ToLong());
			}

			public bool IsPCValid()
			{
				return (Simulator.GetSignalState("UUT/instr_valid(1)").ToLong()) > 0;
			}

			public bool IsNextPCValid()
			{
				return (Simulator.GetSignalState("UUT/instr_valid(0)").ToLong()) > 0;
			}

			public bool IsPipelineStalled()
			{
				return (Simulator.GetSignalState("UUT/im/stall").ToLong()) > 0;
			}

			public int GetRegister(int index)
			{
				if (index <= 31)
				{
					return (int)(Simulator.GetSignalState("UUT/gprf/ISO_REG_FILE_INST/ram(" + index.ToString() + ")(7:0)").Flip().ToLong());
				}
				throw new ArgumentOutOfRangeException("Only 32 registers (0-31)");
			}

			public int GetStatusRegister()
			{
				return (int)(Simulator.GetSignalState("UUT/state_1.rs(0)").Flip().ToLong());
			}
		}

		public class TestBench
		{
			public class Assertion
			{
				public string Contents { get; set; }

				public int Address { get; set; }

				private static Regex assertionOperation = new Regex("(?<a>.*?)(?<op>(==|!=))(?<b>.*)", RegexOptions.IgnoreCase);
				public bool TestAssertion(ProcessorState state)
				{
					Match m = assertionOperation.Match(Contents);
					if (m.Success)
					{
						string a = m.Groups["a"].Value.Trim();
						string b = m.Groups["b"].Value.Trim();
						string op = m.Groups["op"].Value.Trim();

						if (string.Compare(op, "==", true) == 0)
						{
							if (GetValueForString(a, state) == GetValueForString(b, state))
							{
								return true;
							}
							return false;
						}
						else if (string.Compare(op, "!=", true) == 0)
						{
							if (GetValueForString(a, state) == GetValueForString(b, state))
							{
								return false;
							}
							return true;
						}
					}
					Console.WriteLine("Malformed assertion! '{0}'", Contents);
					return false;
				}

				Regex register = new Regex(@"^r(?<index>\d{1,2})", RegexOptions.IgnoreCase | RegexOptions.Multiline);
				public int GetValueForString(string str, ProcessorState state)
				{
					Match m = register.Match(str);
					if (m.Success)
					{
						int i = int.Parse(m.Groups["index"].Value);
						return state.GetRegister(i);
					}

					if (string.Compare(str, "sreg", true) == 0)
					{
						return state.GetStatusRegister();
					}

					int value = 0;
					if (int.TryParse(str, out value))
					{
						return value;
					}

					if (str.StartsWith("0x"))
					{
						if (int.TryParse(str.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value))
						{
							return value;
						}
					}

					bool valueBool = false;
					if (bool.TryParse(str, out valueBool))
					{
						return valueBool ? 1 : 0;
					}

					throw new Exception("parsing exception?");
					// unknown
					return -1;
				}
			}

			private List<string> instructionsList;
			private List<Assertion> assertions;

			public int failedAssertions = 0;
			public int passedAssertions = 0;

			public TestBench()
			{
				instructionsList = new List<string>();
				assertions = new List<Assertion>();
			}

			/// <summary>
			/// Returns true if the processor should cycle again
			/// </summary>
			/// <param name="state"></param>
			/// <returns></returns>
			public bool ProcessTestBench(ProcessorState state)
			{
				int realPC = state.GetNextPC() / 2;
				if (realPC < instructionsList.Count)
				{
					Console.WriteLine("avrPC = {0:X2} -> {1}", realPC, instructionsList[realPC]);
				}
				else
				{
					Console.WriteLine("avrPC = {0:X2} -> End of Test Bench", realPC);
				}

				// find all assertions to be made
				foreach (Assertion a in assertions)
				{
					if (a.Address == realPC)
					{
						Console.Write("assertion\t'{0}' is being tested", a.Contents);
						if (a.TestAssertion(state))
						{
							passedAssertions++;
							Console.WriteLine("\t\tpassed!");
						}
						else
						{
							failedAssertions++;
							Console.WriteLine("\t\tfailed!");
						}
					}
				}

				if (realPC < instructionsList.Count)
				{
					return true;
				}
				return false;
			}

			private static Regex assertionRegex = new Regex(@"#assert\((?<content>.*?)\)", RegexOptions.IgnoreCase);
			public static TestBench Load(string file)
			{
				TestBench bench = new TestBench();

				using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						while (!reader.EndOfStream)
						{
							string line = reader.ReadLine().Trim();

							if (!string.IsNullOrEmpty(line))
							{
								if (line.StartsWith("#"))
								{
									Match m = assertionRegex.Match(line);
									if (m.Success)
									{
										Console.WriteLine("got assertion '{0}'", m.Groups["content"].Value);
										bench.assertions.Add(new Assertion() { Contents = m.Groups["content"].Value, Address = bench.instructionsList .Count});
									}
								}
								else
								{
									Console.WriteLine("got instruction '{0}'", line);
									bench.instructionsList.Add(line);
								}
							}
						}
					}
				}

				return bench;
			}

			public string GenerateAssembly()
			{
				StringBuilder builder = new StringBuilder();
				int instruction = 0;

				foreach (string i in instructionsList)
				{
					builder.AppendLine(i + "   ; INSTRUCTION " + instruction++.ToString());
				}

				return builder.ToString();
			}
		}

		public static MemoryStream GenerateMachineCode(string workingDirectory, string asmFile)
		{
			Console.WriteLine("Starting Assembly -> Machine Code generation using avr-gcc");

			ProcessHelper.ExecuteProcess(workingDirectory, "avr-gcc", "-x assembler-with-cpp \"" + Path.GetFullPath(asmFile) + "\" -nostartfiles -nodefaultlibs");
			ProcessHelper.ExecuteProcess(workingDirectory, "avr-objcopy", "-O binary a.out a.bin");

			MemoryStream stream = new MemoryStream();
			using (FileStream reader = new FileStream(PathHelper.Combine(workingDirectory, "a.bin"), FileMode.Open, FileAccess.Read))
			{
				int b = 0;
				while ((b = reader.ReadByte()) != -1)
				{
					stream.WriteByte((byte)b);
				}
			}

			return stream;
		}

		public static string TestBenchTemplate(MemoryStream code, string templateFile)
		{
			StringBuilder data = new StringBuilder();
			string template = File.ReadAllText(templateFile);
			int currentAddress = 0;

			code.Seek(0, SeekOrigin.Begin);

			int currentBlockIndex = 0;
			int currentBlock = 0;
			int currentData = 0;
			while ((currentData = code.ReadByte())  != -1)
			{
				if (currentBlockIndex == 0)
				{
					currentBlockIndex = 1;
					// 
					currentBlock = currentData;
				}
				else
				{
					currentBlockIndex = 0;
					currentBlock |= currentData << 8;
					data.AppendLine(string.Format("\t\t\tipif_addr_data_pair_format(x\"{0:X4}\", x\"{1:X4}\"),", currentAddress, currentBlock));
					currentAddress += 2;
				}
			}

			// terminate array
			data.AppendLine("ipif_addr_data_pair_format(x\"FFFF\", x\"0000\")");

			return template.Replace("#DATAARRAY", data.ToString());
		}

		public static void PrintRegisterFile(ProcessorState state)
		{
			Console.WriteLine("Current Register Values:");
			for (int i = 0; i < 32; i++)
			{
				int data = state.GetRegister(i);
				Console.WriteLine("\t[{0,2}] {1} (0x{2:X2})", i, data.ToString(), data);
			}
		}
	}
}
