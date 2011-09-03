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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HDLToolkit
{
	public static class ProcessHelper
	{
		public class ProcessListener : IDisposable
		{
			public event Action<string> StdOutNewLineReady;
			public event Action<string> StdErrNewLineReady;

			Process process = null;

			public ProcessListener(Process proc)
			{
				process = proc;
			}

			public void Begin()
			{
				if (process == null)
					return;

				process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
				process.ErrorDataReceived += new DataReceivedEventHandler(process_ErrorDataReceived);

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}

			public void Dispose()
			{
				process.OutputDataReceived -= process_OutputDataReceived;
				process.ErrorDataReceived -= process_ErrorDataReceived;

				process.CancelOutputRead();
				process.CancelErrorRead();

				process = null;
			}

			private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
			{
				if (StdOutNewLineReady != null)
				{
					StdOutNewLineReady(e.Data);
				}
			}

			private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
			{
				if (StdErrNewLineReady != null)
				{
					StdErrNewLineReady(e.Data);
				}
			}
		}

		public class ProcessExecutionResult
		{
			public string StandardOutput { get; set; }
			public string StandardError { get; set; }
		}

		public static ProcessExecutionResult ExecuteProcess(string workingDirectory, string filepath, List<string> arguments)
		{
			string args = "";
			foreach (string arg in arguments)
			{
				if (!string.IsNullOrEmpty(args))
				{
					args += " ";
				}
				args += arg;
			}

			return ExecuteProcess(workingDirectory, filepath, args);
		}

		public static ProcessExecutionResult ExecuteProcess(string workingDirectory, string filepath, string arguments)
		{
			ProcessExecutionResult result = new ProcessExecutionResult();
			Process process = new Process();
			process.StartInfo.FileName = filepath;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.WorkingDirectory = workingDirectory;

			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;

			

			StringBuilder testLogOut = new StringBuilder();
			StringBuilder testLogErr = new StringBuilder();
			ProcessListener listen = new ProcessListener(process);
			listen.StdOutNewLineReady += ((obj) => testLogOut.AppendLine("stdout:" + obj)); // Log StdOut
			listen.StdErrNewLineReady += ((obj) => testLogErr.AppendLine("stderror:" + obj)); // Log StdError

			process.Start();
			listen.Begin();
			process.WaitForExit();

			result.StandardError = testLogErr.ToString();
			result.StandardOutput = testLogOut.ToString();

			listen.Dispose();
			process.Dispose();

			return result;
		}
	}
}
