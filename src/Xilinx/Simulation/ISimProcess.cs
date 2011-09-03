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
using HDLToolkit.ConsoleCommands;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace HDLToolkit.Xilinx.Simulation
{
	public class ISimProcess
	{
		private Process process;
		private string workingDirectory;
		private string executable;

		private List<string> commandOutputs;
		private StringBuilder commandLog;

		private object processLock = new object();
		private bool promptReady = false;

		public bool RunGraphicalUserInterface { get; set; }
		public bool PromptReady
		{
			get
			{
				lock (processLock)
				{
					return promptReady;
				}
			}
		}

		public bool Running
		{
			get
			{
				if (process == null || process.HasExited)
				{
					return false;
				}
				return true;
			}
		}

		public ISimProcess(string workingDirectory, string executable)
		{
			this.workingDirectory = workingDirectory;
			this.executable = executable;
		}

		public void Start()
		{
			if (process != null)
			{
				throw new Exception("Process is already running");
			}

			process = XilinxHelper.CreateXilinxEnvironmentProcess();
			List<string> arguments = new List<string>();
			if (RunGraphicalUserInterface)
			{
				arguments.Add("-gui");
			}

			process.StartInfo.WorkingDirectory = workingDirectory;
			process.StartInfo.Arguments = string.Join(" ", arguments.ToArray());
			process.StartInfo.FileName = executable;

			if (!File.Exists(process.StartInfo.FileName))
			{
				Logger.Instance.WriteError("Executable missing???");
			}

			if (!RunGraphicalUserInterface)
			{
				// Setup redirections
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

				process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);
				process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
			}

			commandOutputs = new List<string>();

			process.Exited += new EventHandler(process_Exited);

			Logger.Instance.WriteDebug("ISim Process starting...");

			process.Start();

			if (!RunGraphicalUserInterface)
			{
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();

				// Inject synchronizing echo request
				InjectCommandNoWait("echo");
			}
		}

		void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Logger.Instance.WriteDebug("[stdout:{0}]", e.Data);

			if (string.Compare(e.Data, "WARNING: A WEBPACK license was found.") == 0)
			{
				Logger.Instance.WriteWarning("ISim License not found, will fall back to Web Pack License");
			}

			if (commandLog != null)
			{
				commandLog.AppendLine(e.Data);
			}
		}

		void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Logger.Instance.WriteDebug("[stderr:{0}]", e.Data);

			// The injection of "echo" commands means that the isim will output a indicator of whether the prompt has returned or not.
			// An echo is inject upon process startup, and again every time a Command is injected (a command from a external caller).
			if (string.Compare(e.Data, "invalid command name \"echo\"") == 0)
			{
				lock (processLock)
				{
					promptReady = true;
				}

				Logger.Instance.WriteVerbose("ISim Prompt is now Ready");

				if (commandLog != null)
				{
					commandOutputs.Add(commandLog.ToString());
				}
				commandLog = new StringBuilder();
			}
			else if (commandLog != null)
			{
				commandLog.AppendLine("stderr:" + e.Data);
			}
		}

		void process_Exited(object sender, EventArgs e)
		{
			Logger.Instance.WriteDebug("Simulation Exited");
			CleanUp();
		}

		private void CleanUp()
		{
			Logger.Instance.WriteDebug("ISim Process cleanup...");

			promptReady = false;
			if (process != null)
			{
				process.Dispose();
			}
			process = null;
		}

		public void Stop()
		{
			if (process == null || process.HasExited)
			{
				return;
			}

			Logger.Instance.WriteDebug("ISim Process terminating...");

			process.Kill();
			process.WaitForExit();

			CleanUp();

			Logger.Instance.WriteDebug("ISim Process terminated");
		}

		private void InjectCommandNoWait(string command)
		{
			lock (processLock)
			{
				promptReady = false;
			}

			process.StandardInput.WriteLine(command);
		}

		public void WaitForPrompt()
		{
			while (true)
			{
				if (process == null || process.HasExited)
				{
					break;
				}

				lock (processLock)
				{
					if (promptReady)
					{
						break;
					}
				}
				Thread.Sleep(100);
			}
		}

		public string InjectCommand(string command)
		{
			if (process == null || process.HasExited)
			{
				throw new Exception("Process is not running");
			}

			WaitForPrompt();

			InjectCommandNoWait(command);
			Thread.Sleep(100);
			InjectCommandNoWait("echo");

			WaitForPrompt();

			string result = string.Join("\n", commandOutputs.ToArray());
			commandOutputs.Clear();
			return result;
		}
	}
}
