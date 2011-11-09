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
	public class StringProcessListener : IProcessListener
	{
		private StringBuilder builderOut;
		private StringBuilder builderErr;

		public string Output { get { return builderOut.ToString(); } }
		public string ErrorOutput { get { return builderErr.ToString(); } }

		public StringProcessListener()
		{
			builderOut = new StringBuilder();
			builderErr = new StringBuilder();
		}

		public void ProcessLine(string line)
		{
			builderOut.AppendLine(line);
		}

		public void ProcessErrorLine(string line)
		{
			builderErr.AppendLine(line);
		}

		public void Clear()
		{
			builderOut = new StringBuilder();
			builderErr = new StringBuilder();
		}

		public void Dispose()
		{
			// Nothing to do here
		}
	}

	public interface IProcessListener : IDisposable
	{
		void ProcessLine(string line);
		void ProcessErrorLine(string line);
	}

	public class XilinxProcess : IDisposable
	{
		public List<IProcessListener> Listeners { get; private set; }
		public Process CurrentProcess { get; private set; }
		private ProcessHelper.ProcessListener listener = null;

		public string Tool { get; set; }
		public List<string> Arguments { get; private set; }
		public string WorkingDirectory { get; set; }

		public bool RedirectOutput { get; set; }
		public bool RedirectInput { get; set; }

		public virtual bool Running
		{
			get
			{
				if (CurrentProcess != null)
				{
					if (!CurrentProcess.HasExited)
					{
						return true;
					}
				}
				return false;
			}
		}

		internal XilinxProcess(string tool, string workingDirectory, List<string> arguments, List<IProcessListener> listeners)
		{
			RedirectOutput = true;
			Listeners = listeners;
			Arguments = arguments;
			WorkingDirectory = workingDirectory;
			Tool = tool;
		}

		public XilinxProcess(string tool)
			: this(tool, Environment.CurrentDirectory, new List<string>(), new List<IProcessListener>())
		{
		}

		public XilinxProcess(string tool, string workingDirectory)
			: this(tool, workingDirectory, new List<string>(), new List<IProcessListener>())
		{
		}

		public XilinxProcess(string tool, List<string> arguments)
			: this(tool, Environment.CurrentDirectory, new List<string>(arguments), new List<IProcessListener>())
		{
		}

		public XilinxProcess(string tool, string workingDirectory, List<string> arguments)
			: this(tool, workingDirectory, new List<string>(arguments), new List<IProcessListener>())
		{
		}

		public static Process CreateXilinxEnvironmentProcess()
		{
			Process process = new Process();
			process.StartInfo.UseShellExecute = false;

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

		public virtual void Start()
		{
			// Check to see if process is already busy
			if (Running)
			{
				throw new Exception("Process is already running.");
			}

			Dispose();
			
			// Get the tool executable
			string toolPath = Tool;
			if (!Path.IsPathRooted(toolPath))
			{
				toolPath = XilinxHelper.GetXilinxToolPath(Tool);
				if (toolPath == null)
				{
					throw new Exception(string.Format("Unable to location the executable for the tool '{0}'", Tool));
				}
			}

			// Create the process with special Xilinx Environment
			CurrentProcess = CreateXilinxEnvironmentProcess();
			CurrentProcess.StartInfo.FileName = toolPath;

			string args = "";
			foreach (string arg in Arguments)
			{
				if (!string.IsNullOrEmpty(args))
				{
					args += " ";
				}
				args += arg;
			}
			CurrentProcess.StartInfo.Arguments = args;
			CurrentProcess.StartInfo.WorkingDirectory = WorkingDirectory;

			CurrentProcess.StartInfo.UseShellExecute = false;
			if (RedirectOutput)
			{
				CurrentProcess.StartInfo.RedirectStandardError = true;
				CurrentProcess.StartInfo.RedirectStandardOutput = true;

				listener = new ProcessHelper.ProcessListener(CurrentProcess);
				listener.StdOutNewLineReady += ((obj) => ProcessLine(obj));
				listener.StdErrNewLineReady += ((obj) => ProcessErrorLine(obj));
			}
			if (RedirectInput)
			{
				CurrentProcess.StartInfo.RedirectStandardInput = true;
			}

			// Start the process
			CurrentProcess.Start();

			if (RedirectOutput)
			{
				// Start the listener
				listener.Begin();
			}
		}

		protected virtual void ProcessLine(string line)
		{
			foreach (IProcessListener listener in Listeners)
			{
				listener.ProcessLine(line);
			}
		}

		protected virtual void ProcessErrorLine(string line)
		{
			foreach (IProcessListener listener in Listeners)
			{
				listener.ProcessErrorLine(line);
			}
		}

		public virtual void Kill()
		{
			if (CurrentProcess != null)
			{
				CurrentProcess.Kill();
			}
		}

		public virtual void WaitForExit()
		{
			if (CurrentProcess != null)
			{
				CurrentProcess.WaitForExit();
			}
		}

		public virtual void Dispose()
		{
			// Dispose of the listener
			if (listener != null)
			{
				listener.Dispose();
			}

			// Dispose of the process
			if (CurrentProcess != null)
			{
				CurrentProcess.Dispose();
			}
		}

		public static ProcessHelper.ProcessExecutionResult ExecuteProcess(string workingDirectory, string tool, List<string> arguments)
		{
			ProcessHelper.ProcessExecutionResult result = new ProcessHelper.ProcessExecutionResult();
			using (XilinxProcess process = new XilinxProcess(tool, arguments))
			{
				StringProcessListener listener = new StringProcessListener();
				process.Listeners.Add(listener);
				process.WorkingDirectory = workingDirectory;

				process.Start();
				process.WaitForExit();

				result.StandardError = listener.ErrorOutput;
				result.StandardOutput = listener.Output;
			}
			return result;
		}
	}
}
