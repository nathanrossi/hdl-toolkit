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
using System.Diagnostics;
using System.IO;

namespace HDLToolkit.Xilinx
{
	public class XilinxProcess : StandardProcess
	{
		public XilinxProcess(string executable)
			: base(executable, Environment.CurrentDirectory)
		{
		}

		public XilinxProcess(string executable, string workingDirectory)
			: base(executable, workingDirectory)
		{
		}

		public XilinxProcess(string executable, List<string> arguments)
			: base(executable, arguments)
		{
		}

		public XilinxProcess(string executable, string workingDirectory, List<string> arguments)
			: base(executable, workingDirectory, arguments)
		{
		}

		protected override Process CreateProcess()
		{
			Process process = base.CreateProcess();
			
			Logger.Instance.WriteDebug("Setting up a process to run inside a Xilinx Environment on a {0} platform.", SystemHelper.GetSystemType());

			List<string> binPaths = new List<string>(XilinxHelper.GetXilinxBinaryPaths());
			List<string> libPaths = new List<string>(XilinxHelper.GetXilinxLibraryPaths());

			// Binary Paths on the PATH environment
			process.StartInfo.EnvironmentVariables["PATH"] =
				SystemHelper.EnvironmentPathPrepend(process.StartInfo.EnvironmentVariables["PATH"], binPaths);

			Logger.Instance.WriteDebug("PATH Environment = '{0}'", process.StartInfo.EnvironmentVariables["PATH"]);

			// Specific the LD_LIBRARY_PATH aswell for *nix systems
			if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Linux)
			{
				Logger.Instance.WriteDebug("Appending Xilinx lib paths to the LD_LIBRARY_PATH");
				process.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"] =
					SystemHelper.EnvironmentPathPrepend(process.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"], libPaths);

				Logger.Instance.WriteDebug("LD_LIBRARY_PATH Environment = '{0}'", process.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"]);
			}

			// Xilinx Specific Environment Variables
			process.StartInfo.EnvironmentVariables["XILINX"] = PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "ISE");
			process.StartInfo.EnvironmentVariables["XILINX_DSP"] = PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "ISE");
			process.StartInfo.EnvironmentVariables["XILINX_EDK"] = PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "EDK");
			process.StartInfo.EnvironmentVariables["XILINX_PLANAHEAD"] = PathHelper.Combine(XilinxHelper.GetRootXilinxPath(), "PlanAhead");

			Logger.Instance.WriteDebug("XILINX Environment = '{0}'", process.StartInfo.EnvironmentVariables["XILINX"]);

			return process;
		}

		protected override string GetExecutablePath(string executable)
		{
			return XilinxHelper.GetXilinxToolPath(executable);
		}

		public static new ProcessHelper.ProcessExecutionResult ExecuteProcess(string workingDirectory, string executable, List<string> arguments)
		{
			using (XilinxProcess process = new XilinxProcess(executable, workingDirectory, arguments))
			{
				return ExecuteProcessObject(process);
			}
		}
	}
}
