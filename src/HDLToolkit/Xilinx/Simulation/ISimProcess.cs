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
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace HDLToolkit.Xilinx.Simulation
{
	public class ISimProcess : XilinxProcess
	{
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

		public ISimProcess(string workingDirectory, string executable)
			: base(executable, workingDirectory)
		{
		}

		public override void Start()
		{
			// Setup the Arguments
			Arguments.Clear();
			if (RunGraphicalUserInterface)
			{
				Arguments.Add("-gui");
			}

			Logger.Instance.WriteDebug("ISim Process starting...");

			this.RedirectInput = true;
			this.RedirectOutput = true;

			base.Start();
		}

		protected override void ProcessLine(string line)
		{
			//Logger.Instance.WriteDebug("[stdout:{0}]", line);
			if (string.Compare(line, "WARNING: A WEBPACK license was found.") == 0)
			{
				Logger.Instance.WriteWarning("ISim License not found, will fall back to Web Pack License");
			}

			if (commandLog != null)
			{
				commandLog.AppendLine(line);
			}
			
			base.ProcessLine(line);
		}

		protected override void ProcessErrorLine(string line)
		{
			//Logger.Instance.WriteDebug("[stderr:{0}]", line);

			// The injection of "echo" commands means that the isim will output a indicator of whether the prompt has returned or not.
			// An echo is inject upon process startup, and again every time a Command is injected (a command from a external caller).
			if (string.Compare(line, "invalid command name \"echo\"") == 0)
			{
				lock (processLock)
				{
					promptReady = true;
				}

				if (commandLog != null)
				{
					commandOutputs.Add(commandLog.ToString());
				}
				commandLog = new StringBuilder();
			}
			else if (commandLog != null)
			{
				commandLog.AppendLine("stderr:" + line);
			}

			base.ProcessErrorLine(line);
		}

		protected override void Exited()
		{
			Logger.Instance.WriteDebug("Simulation Exited");

			base.Exited();
		}

		public override void Dispose()
		{
			Logger.Instance.WriteDebug("ISim Process cleanup...");

			promptReady = false;
			
			base.Dispose();
		}

		private void InjectCommandNoWait(string command)
		{
			lock (processLock)
			{
				promptReady = false;
			}

			CurrentProcess.StandardInput.WriteLine(command);
		}

		public void WaitForPrompt()
		{
			while (true)
			{
				if (CurrentProcess == null || CurrentProcess.HasExited)
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
			}
		}

		public string InjectCommand(string command)
		{
			if (CurrentProcess == null || CurrentProcess.HasExited)
			{
				throw new Exception("Process is not running");
			}

			WaitForPrompt();

			InjectCommandNoWait(command);
			//Thread.Sleep(10);
			InjectCommandNoWait("echo");

			WaitForPrompt();

			string result = string.Join("\n", commandOutputs.ToArray());
			commandOutputs.Clear();
			return result;
		}
	}
}
