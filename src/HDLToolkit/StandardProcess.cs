using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Xilinx;
using System.IO;
using System.Diagnostics;

namespace HDLToolkit
{
	public class StandardProcess : IDisposable
	{
		public List<IProcessListener> Listeners { get; private set; }
		public Process CurrentProcess { get; private set; }
		private ProcessHelper.ProcessListener listener = null;

		public string Executable { get; set; }
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

		public StandardProcess(string executable)
			: this(executable, Environment.CurrentDirectory)
		{
		}

		public StandardProcess(string executable, string workingDirectory)
		{
			RedirectOutput = true;
			Arguments = new List<string>();
			Listeners = new List<IProcessListener>();
			WorkingDirectory = workingDirectory;
			Executable = executable;
		}

		public StandardProcess(string executable, List<string> arguments)
			: this(executable, Environment.CurrentDirectory, arguments)
		{
		}

		public StandardProcess(string executable, string workingDirectory, List<string> arguments)
			: this(executable, workingDirectory)
		{
			if (arguments != null)
			{
				Arguments.AddRange(arguments);
			}
		}

		protected virtual Process CreateProcess()
		{
			return new Process();
		}

		protected virtual string GetExecutablePath(string executable)
		{
			return null;
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
			string executablePath = Executable;
			if (!Path.IsPathRooted(executablePath))
			{
				executablePath = GetExecutablePath(Executable);
				if (executablePath == null)
				{
					executablePath = Executable;
				}
			}

			// Create the process
			CurrentProcess = CreateProcess();
			CurrentProcess.StartInfo.FileName = executablePath;
			CurrentProcess.StartInfo.WorkingDirectory = WorkingDirectory;

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

			CurrentProcess.StartInfo.UseShellExecute = false;
			if (RedirectOutput)
			{
				CurrentProcess.StartInfo.RedirectStandardError = true;
				CurrentProcess.StartInfo.RedirectStandardOutput = true;

				listener = new ProcessHelper.ProcessListener(CurrentProcess);
				listener.StdOutNewLineReady += delegate(string line) { if (line != null) { ProcessLine(line); } };
				listener.StdErrNewLineReady += delegate(string line) { if (line != null) { ProcessErrorLine(line); } };
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

		public virtual void ProcessWriteLine(string line)
		{
			if (CurrentProcess != null && RedirectInput && Running)
			{
				CurrentProcess.StandardInput.WriteLine(line);
			}
		}

		public virtual void Kill()
		{
			if (CurrentProcess != null)
			{
				if (!CurrentProcess.HasExited)
				{
					CurrentProcess.Kill();
				}
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
				listener = null;
			}

			// Dispose of the process
			if (CurrentProcess != null)
			{
				CurrentProcess.Dispose();
				CurrentProcess = null;
			}
		}

		protected static ProcessHelper.ProcessExecutionResult ExecuteProcessObject(StandardProcess process)
		{
			using (StringProcessListener listener = new StringProcessListener())
			{
				ProcessHelper.ProcessExecutionResult result = new ProcessHelper.ProcessExecutionResult();
				process.Listeners.Add(listener);
				process.RedirectOutput = true;

				process.Start();
				process.WaitForExit();

				result.StandardError = listener.ErrorOutput;
				result.StandardOutput = listener.Output;

				return result;
			}
		}

		public static ProcessHelper.ProcessExecutionResult ExecuteProcess(string workingDirectory, string executable, List<string> arguments)
		{
			using (StandardProcess process = new StandardProcess(executable, workingDirectory, arguments))
			{
				return ExecuteProcessObject(process);
			}
		}
	}
}
