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
			bool debugEnable = false;
			bool guiEnable = false;
			List<string> files = new List<string>();
			foreach (string a in args)
			{
				if (string.Compare(a, "-d", true) == 0)
				{
					debugEnable = true;
				}
				else if (string.Compare(a, "-g", true) == 0)
				{
					guiEnable = true;
				}
				else
				{
					files.Add(a);
				}
			}

			if (debugEnable)
			{
				Logger.Instance.VerbosityLevel = Logger.Verbosity.Debug;
			}

			XilinxRepository repo = new XilinxRepository();
			repo.AddSearchPath(PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "EDK", "hw"));
			repo.AddSearchPath(@"C:\svn\uni-projects\uqrarg\hardware\Repo");

			string testRoot = PathHelper.Combine(repo.GetLibraryDefaultRootPath("avr_core_v1_00_a"), "test");

			foreach (string file in files)
			{
				string fullFilePath = file;
				if (!Path.IsPathRooted(fullFilePath))
				{
					fullFilePath = PathHelper.Combine(testRoot, file);
				}

				if (File.Exists(fullFilePath))
				{
					try
					{
						TestRunner runner = new TestRunner(repo, fullFilePath);
						if (guiEnable)
						{
							runner.GuiEnabled = true;
						}
						runner.Run();
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception {0}", ex.Message);
						Console.WriteLine("{0}", ex.StackTrace);
						Console.WriteLine("Continuing...");
					}
				}
				else
				{
					Logger.Instance.WriteError("{0} does not exist", Path.GetFileName(fullFilePath));
				}
			}
		}
	}
}
