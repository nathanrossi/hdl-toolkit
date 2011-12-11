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
using NConsole;
using System.Reflection;

namespace HDLToolkit.Console.Commands
{
	[Command("help")]
	public class HelpCommand : ICommand
	{
		[Argument(Position = 0)]
		public string Command { get; set; }

		private string exeName;

		public void Execute()
		{
			AssemblyTitleAttribute assemblyTitle = (AssemblyTitleAttribute)Assembly.GetExecutingAssembly().
				GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault();
			AssemblyName name = Assembly.GetExecutingAssembly().GetName();
			exeName = name.Name;

			string header = string.Format("{0} - {1}", assemblyTitle.Title, name.Version);
			Logger.Instance.WriteInfo(header);
			Logger.Instance.WriteInfo("");

			switch (Command)
			{
				case "coreprjgen":
					PrintCorePrjGen();
					break;
				case "corexisegen":
					PrintCoreXiseGen();
					break;
				case "coretree":
					PrintCoreTree();
					break;
				case "deviceinfo":
					PrintDeviceInfo();
					break;
				case "clearcache":
					PrintClearCache();
					break;
				default:
					PrintMainHelp();
					break;
			}
		}

		public void PrintMainHelp()
		{
			Logger.Instance.WriteInfo("Usage: {0} command [options, parameters, ...]", exeName);
			Logger.Instance.WriteInfo("");
			Logger.Instance.WriteInfo("Valid Commands:");
			Logger.Instance.WriteInfo("\thelp               The help command, displays this page.");
			Logger.Instance.WriteInfo("\tcoreprjgen         Generate a Xilinx 'prj' file for the specified cores.");
			Logger.Instance.WriteInfo("\tcorexisegen        Generate a Xilinx ISE file for the specified cores.");
			Logger.Instance.WriteInfo("\tcoretree           Generate a tree in the console showing the specified cores hiearchy.");
			Logger.Instance.WriteInfo("\tdeviceinfo         Query information for a device/family/part/package/etc.");
			Logger.Instance.WriteInfo("\tclearcache         Clears all cached data, run this command when you have updated a toolchain.");
			Logger.Instance.WriteInfo("");
			Logger.Instance.WriteInfo("Common Options:");
			Logger.Instance.WriteInfo("\t-v [--verbose]");
			Logger.Instance.WriteInfo("\t\tVerbose output.");
			Logger.Instance.WriteInfo("\t-r=<directory> [--repo=<directory>]");
			Logger.Instance.WriteInfo("\t\tSpecifies an Additional Repository to look in.");
			Logger.Instance.WriteInfo("\t--disable-environment-repo");
			Logger.Instance.WriteInfo("\t\tDisable the 'REPOSITORY' environment search path.");
			Logger.Instance.WriteInfo("");
			Logger.Instance.WriteInfo("'{0} help [command]' for more details.", exeName);
		}

		public void PrintCorePrjGen()
		{
			Logger.Instance.WriteInfo("Usage: {0} coreprjgen core-name ... --output=<file>", exeName);
			Logger.Instance.WriteInfo("");
			Logger.Instance.WriteInfo("Options:");
			Logger.Instance.WriteInfo("\t-o=<file> [--output=<file>]");
			Logger.Instance.WriteInfo("\t\tSpecifies the output file (Required)");
			Logger.Instance.WriteInfo("\t--sim-only");
			Logger.Instance.WriteInfo("\t\tSpecifies that the project should include only Simulation modules.");
			Logger.Instance.WriteInfo("\t--syn-only");
			Logger.Instance.WriteInfo("\t\tSpecifies that the project should include only Synthesis modules.");
		}

		public void PrintCoreXiseGen()
		{
			Logger.Instance.WriteInfo("Usage: {0} corexisegen core-name ... --output=<file>", exeName);
			Logger.Instance.WriteInfo("");
			Logger.Instance.WriteInfo("Options:");
			Logger.Instance.WriteInfo("\t-o=<file> [--output=<file>]");
			Logger.Instance.WriteInfo("\t\tSpecifies the output file (Required)");
			Logger.Instance.WriteInfo("\t-u=<file> [--ucf=<file>]");
			Logger.Instance.WriteInfo("\t\tSpecifies user constraints file (*.ucf) (Optional)");
			Logger.Instance.WriteInfo("\t-d=<device> [--device=<device>]");
			Logger.Instance.WriteInfo("\t\tSpecifies the device/package/speed, e.g. 'xc3s100e-5vq100' (Optional)");
		}

		public void PrintCoreTree()
		{
			Logger.Instance.WriteInfo("Usage: {0} coretree core-name ... [-m, -c]", exeName);
			Logger.Instance.WriteInfo("");
			Logger.Instance.WriteInfo("Options:");
			Logger.Instance.WriteInfo("\t-m [--display-modules]");
			Logger.Instance.WriteInfo("\t\tDisplays the modules for each core in the tree.");
			Logger.Instance.WriteInfo("\t-c [--display-components]");
			Logger.Instance.WriteInfo("\t\tDisplays the components for each module in the tree.");
		}

		public void PrintDeviceInfo()
		{
			Logger.Instance.WriteInfo("Usage: {0} deviceinfo query-string", exeName);
			Logger.Instance.WriteInfo("");
		}

		public void PrintClearCache()
		{
			Logger.Instance.WriteInfo("Usage: {0} clearcache", exeName);
			Logger.Instance.WriteInfo("");
		}
	}
}
