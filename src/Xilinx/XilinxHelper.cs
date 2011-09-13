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
using System.IO;
using HDLToolkit.ConsoleCommands;
using System.Diagnostics;

namespace HDLToolkit.Xilinx
{
	public static class XilinxHelper
	{
		// Cached lookup path
		public static string XilinxPath = null;

		private const string XilinxDefaultDirectory_Windows = "C:\\Xilinx";
		private const string XilinxDefaultDirectory_Linux = "/opt/Xilinx";

		private static string GetXilinxDefaultRoot()
		{
			if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Windows)
			{
				return XilinxDefaultDirectory_Windows;
			}
			else if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Linux)
			{
				return XilinxDefaultDirectory_Linux;
			}

			// Only Supporting Windows/Linux
			throw new NotSupportedException();
		}

		public static string GetRootXilinxPath()
		{
			if (!string.IsNullOrEmpty(XilinxPath))
			{
				return XilinxPath;
			}

			string rootISEPath = Environment.GetEnvironmentVariable("XILINX");
			if (string.IsNullOrEmpty(rootISEPath))
			{
				string[] versions = Directory.GetDirectories(GetXilinxDefaultRoot());
				float highest_float = 0;
				string highest = null;
				foreach (string version in versions)
				{
					string version_str = Path.GetFileName(version);
					float version_float = float.Parse(version_str);
					if (highest == null || version_float > highest_float)
					{
						highest_float = version_float;
						highest = version_str;
					}
				}

				if (highest != null)
				{
					string realPath = PathHelper.Combine(GetXilinxDefaultRoot(), highest);
					// Check if the version is equal or above 12.1.
					if (highest_float >= 12.1)
					{
						// In this version another sub-directory is added.
						realPath = PathHelper.Combine(realPath, "ISE_DS");
					}

					rootISEPath = realPath;
					XilinxPath = rootISEPath;

					Logger.Instance.WriteVerbose("Located Xilinx {0} root at '{1}'", highest, XilinxPath);
					return rootISEPath;
				}
				else
				{
					throw new Exception("Unable to locate a Xilinx Installation directory, please set the XILINX variable.");
				}
			}
			rootISEPath = Path.GetFullPath(PathHelper.Combine(rootISEPath, ".."));
			XilinxPath = rootISEPath;
			Logger.Instance.WriteVerbose("Located Xilinx root at '{0}'", XilinxPath);
			return rootISEPath;
		}

		public static string GetPlatformAndArchPath()
		{
			// TODO Make this a little smarter
			if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Windows)
			{
				if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(GetRootXilinxPath(), "ISE", "bin", "nt64"))))
				{
					// 64 Bit Platform is Installed
					return "nt64";
				}
				else if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(GetRootXilinxPath(), "ISE", "bin", "nt"))))
				{
					// 32 Bit Platform fallback
					return "nt";
				}
			}
			else if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Linux)
			{
				if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(GetRootXilinxPath(), "ISE", "bin", "lin64"))))
				{
					// 64 Bit Platform is Installed
					return "lin64";
				}
				else if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(GetRootXilinxPath(), "ISE", "bin", "lin"))))
				{
					// 32 Bit Platform fallback
					return "lin";
				}
			}

			throw new NotSupportedException("Xilinx is not installed, or your Platform and or Architecture are not supported.");
		}

		public static string[] GetXilinxBinaryPaths()
		{
			string root = GetRootXilinxPath();
			string platformArch = GetPlatformAndArchPath();
			
			return new string[] {
				Path.GetFullPath(PathHelper.Combine(root, "ISE", "bin", platformArch)),
				Path.GetFullPath(PathHelper.Combine(root, "EDK", "bin", platformArch)),
				Path.GetFullPath(PathHelper.Combine(root, "common", "bin", platformArch))};
		}

		public static string[] GetXilinxLibraryPaths()
		{
			string root = GetRootXilinxPath();
			string platformArch = GetPlatformAndArchPath();

			return new string[] {
					Path.GetFullPath(PathHelper.Combine(root, "ISE", "lib", platformArch)),
					Path.GetFullPath(PathHelper.Combine(root, "EDK", "lib", platformArch)),
					Path.GetFullPath(PathHelper.Combine(root, "common", "lib", platformArch))};
		}

		public static string GetXilinxBinaryPath(string filename)
		{
			string[] binaryPaths = GetXilinxBinaryPaths();

			foreach (string path in binaryPaths)
			{
				string expanded = PathHelper.Combine(path, filename);
				if (File.Exists(expanded))
				{
					return expanded;
				}
			}

			return null;
		}

		public static Process CreateXilinxEnvironmentProcess()
		{
			Process process = new Process();
			process.StartInfo.UseShellExecute = false;

			Logger.Instance.WriteDebug("Setting up a process to run inside a Xilinx Environment on a {0} platform.", SystemHelper.GetSystemType());

			List<string> binPaths = new List<string>(GetXilinxBinaryPaths());
			List<string> libPaths = new List<string>(GetXilinxLibraryPaths());

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
			process.StartInfo.EnvironmentVariables["XILINX"] = PathHelper.Combine(GetRootXilinxPath(), "ISE");
			process.StartInfo.EnvironmentVariables["XILINX_DSP"] = PathHelper.Combine(GetRootXilinxPath(), "ISE");
			process.StartInfo.EnvironmentVariables["XILINX_EDK"] = PathHelper.Combine(GetRootXilinxPath(), "EDK");
			process.StartInfo.EnvironmentVariables["XILINX_PLANAHEAD"] = PathHelper.Combine(GetRootXilinxPath(), "PlanAhead");

			Logger.Instance.WriteDebug("XILINX Environment = '{0}'", process.StartInfo.EnvironmentVariables["XILINX"]);

			return process;
		}

		public static ProcessHelper.ProcessExecutionResult ExecuteProcess(string workingDirectory, string filepath, List<string> arguments)
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

		public static ProcessHelper.ProcessExecutionResult ExecuteProcess(string workingDirectory, string filepath, string arguments)
		{
			ProcessHelper.ProcessExecutionResult result = new ProcessHelper.ProcessExecutionResult();
			Process process = CreateXilinxEnvironmentProcess();
			process.StartInfo.FileName = filepath;
			process.StartInfo.Arguments = arguments;
			process.StartInfo.WorkingDirectory = workingDirectory;

			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;

			StringBuilder testLogOut = new StringBuilder();
			StringBuilder testLogErr = new StringBuilder();
			ProcessHelper.ProcessListener listen = new ProcessHelper.ProcessListener(process);
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
