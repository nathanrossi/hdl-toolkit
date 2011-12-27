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
using System.Diagnostics;

namespace HDLToolkit.Xilinx
{
	public static class XilinxHelper
	{
		// Cached lookup path
		public static string XilinxPath = null;
		public static string[] XilinxBinaryPaths = null;
		public static string[] XilinxLibraryPaths = null;

		public static List<XilinxVersion> XilinxInstalls = null;
		public static XilinxVersion XilinxEnviromentVersion = null;
		public static XilinxVersion CurrentVersion = null;

		private const string XilinxDefaultDirectory_Windows = @"C:\Xilinx";
		private const string XilinxDefaultDirectory_Linux = @"/opt/Xilinx";

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

		/// <summary>
		/// Scan the system for Xilinx installs using default install information.
		/// </summary>
		private static void ScanForXilinxInstalls()
		{
			if (XilinxInstalls == null)
			{
				XilinxInstalls = new List<XilinxVersion>();

				// Check the environment variable for a forced XILINX path
				string environmentISE = Environment.GetEnvironmentVariable("XILINX");
				if (Directory.Exists(environmentISE))
				{
					XilinxEnviromentVersion = XilinxVersion.GetVersionFromFileset(environmentISE);
					if (XilinxEnviromentVersion != null)
					{
						Logger.Instance.WriteDebug("Install version {0} @ '{1}' id = '{2}'",
							XilinxEnviromentVersion.Version, XilinxEnviromentVersion.RootPath, XilinxEnviromentVersion.UniqueId);
						XilinxInstalls.Add(XilinxEnviromentVersion);
					}
				}

				// Scan for extra installs
				string[] installs = Directory.GetDirectories(GetXilinxDefaultRoot());
				foreach (string install in installs)
				{
					XilinxVersion installVersion = XilinxVersion.GetVersionFromFileset(install);
					if (installVersion == null)
					{
						// May use a ISE_DS sub directory
						installVersion = XilinxVersion.GetVersionFromFileset(PathHelper.Combine(install, "ISE_DS"));
					}

					if (installVersion != null)
					{
						Logger.Instance.WriteDebug("Install version {0} @ '{1}' id = '{2}'",
							installVersion.Version, installVersion.RootPath, installVersion.UniqueId);
						// TODO: avoid duplicates
						XilinxInstalls.Add(installVersion);
					}
				}
			}
		}

		public static XilinxVersion GetCurrentXilinxVersion()
		{
			if (CurrentVersion != null)
			{
				return CurrentVersion;
			}
			else
			{
				ScanForXilinxInstalls();

				if (XilinxEnviromentVersion == null)
				{
					foreach (XilinxVersion version in XilinxInstalls)
					{
						if (CurrentVersion == null || version.Version > CurrentVersion.Version)
						{
							CurrentVersion = version;
						}
					}
				}
				else
				{
					CurrentVersion = XilinxEnviromentVersion;
				}
			}

			if (CurrentVersion == null)
			{
				throw new Exception("Unable to locate a Xilinx Installation directory, please set the XILINX variable.");
			}
			Logger.Instance.WriteVerbose("Located Xilinx {0} root at '{1}'", CurrentVersion.Version, CurrentVersion.RootPath);

			return CurrentVersion;
		}

		public static string GetRootXilinxPath()
		{
			return GetCurrentXilinxVersion().RootPath;
		}

		public static string[] GetXilinxBinaryPaths()
		{
			if (XilinxBinaryPaths == null)
			{
				string root = GetRootXilinxPath();
				string platformArch = GetPlatformAndArchPath();

				XilinxBinaryPaths = new string[] {
					Path.GetFullPath(PathHelper.Combine(root, "ISE", "bin", platformArch)),
					Path.GetFullPath(PathHelper.Combine(root, "EDK", "bin", platformArch)),
					Path.GetFullPath(PathHelper.Combine(root, "common", "bin", platformArch))};
			}
			return XilinxBinaryPaths;
		}

		private static string GetPlatformAndArchPath()
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

		public static string[] GetXilinxLibraryPaths()
		{
			if (XilinxLibraryPaths == null)
			{
				string root = GetRootXilinxPath();
				string platformArch = GetPlatformAndArchPath();

				XilinxLibraryPaths = new string[] {
					Path.GetFullPath(PathHelper.Combine(root, "ISE", "lib", platformArch)),
					Path.GetFullPath(PathHelper.Combine(root, "EDK", "lib", platformArch)),
					Path.GetFullPath(PathHelper.Combine(root, "common", "lib", platformArch))};
			}
			return XilinxLibraryPaths;
		}

		/// <summary>
		/// Retrieve the full path to the executable for the tool.
		/// </summary>
		public static string GetXilinxToolPath(string tool)
		{
			string[] binaryPaths = GetXilinxBinaryPaths();
			string toolRawName = Path.GetFileNameWithoutExtension(tool);

			foreach (string path in binaryPaths)
			{
				string expanded = PathHelper.Combine(path, toolRawName);
				// On windows executables have the ".exe" extension
				if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Windows)
				{
					expanded = expanded + ".exe";
				}
				if (File.Exists(expanded))
				{
					return expanded;
				}
			}

			return null;
		}
	}
}
