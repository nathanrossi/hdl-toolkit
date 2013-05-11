using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework;
using System.IO;
using HDLToolkit.Framework.Synthesis;
using HDLToolkit.Framework.Implementation;
using HDLToolkit.Framework.Simulation;
using HDLToolkit.Framework.Devices;
using HDLToolkit.Xilinx.Synthesis;
using HDLToolkit.Xilinx.Implementation;
using HDLToolkit.Xilinx.Devices;

namespace HDLToolkit.Xilinx
{
	public class XilinxToolchain : IToolchain
	{
		public ToolchainManager Manager { get; private set; }
		public IToolchainVersion Version { get; private set; }
		public string UniqueId { get { return Version.UniqueId; } }

		// Tool Paths
		public string RootPath
		{
			get { return (Version as XilinxVersion).RootPath; }
		}
		private string PlatformArchPath { get; set; }
		public IEnumerable<string> BinaryPaths { get; private set; }
		public IEnumerable<string> LibraryPaths { get; private set; }

		public IEnumerable<ISimulator> Simulators { get; private set; }
		public IEnumerable<ISynthesizer> Synthesizers { get; private set; }
		public IEnumerable<IImplementor> Implementors { get; private set; }

		public XilinxToolchain(ToolchainManager manager, XilinxVersion version)
		{
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			Version = version;
			Manager = manager;

			Logger.Instance.WriteDebug("Created Xilinx Toolchain Instance with version '{0}'", Version.ToString());

			PopulatePathData();
			PopulateToolFactories();
		}

		#region Configuration
		private void PopulatePathData()
		{
			PlatformArchPath = GetPlataformArchitecturePath();

			HashSet<string> binaryPaths = new HashSet<string>();
			binaryPaths.Add(Path.GetFullPath(PathHelper.Combine(RootPath, "ISE", "bin", PlatformArchPath)));
			binaryPaths.Add(Path.GetFullPath(PathHelper.Combine(RootPath, "EDK", "bin", PlatformArchPath)));
			binaryPaths.Add(Path.GetFullPath(PathHelper.Combine(RootPath, "common", "bin", PlatformArchPath)));
			BinaryPaths = binaryPaths;

			HashSet<string> libraryPaths = new HashSet<string>();
			libraryPaths.Add(Path.GetFullPath(PathHelper.Combine(RootPath, "ISE", "lib", PlatformArchPath)));
			libraryPaths.Add(Path.GetFullPath(PathHelper.Combine(RootPath, "EDK", "lib", PlatformArchPath)));
			libraryPaths.Add(Path.GetFullPath(PathHelper.Combine(RootPath, "common", "lib", PlatformArchPath)));
			LibraryPaths = libraryPaths;
		}

		private string GetPlataformArchitecturePath()
		{
			// TODO Make this a little smarter (allow use of 32bit on 64bit system)
			if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Windows)
			{
				if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(RootPath, "ISE", "bin", "nt64"))))
				{
					// 64 Bit Platform is Installed
					return "nt64";
				}
				else if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(RootPath, "ISE", "bin", "nt"))))
				{
					// 32 Bit Platform fallback
					return "nt";
				}
			}
			else if (SystemHelper.GetSystemType() == SystemHelper.SystemType.Linux)
			{
				if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(RootPath, "ISE", "bin", "lin64"))))
				{
					// 64 Bit Platform is Installed
					return "lin64";
				}
				else if (Directory.Exists(Path.GetFullPath(PathHelper.Combine(RootPath, "ISE", "bin", "lin"))))
				{
					// 32 Bit Platform fallback
					return "lin";
				}
			}
			throw new NotSupportedException("This Xilinx install is corrupt or does not support your Architecture and/or Platform.");
		}

		private void PopulateToolFactories()
		{
			HashSet<ISimulator> simulators = new HashSet<ISimulator>();
			//simulators.Add(new XSTSynthesizer(this));
			Simulators = simulators;

			HashSet<ISynthesizer> synthesizers = new HashSet<ISynthesizer>();
			synthesizers.Add(new XSTSynthesizer(this));
			Synthesizers = synthesizers;

			HashSet<IImplementor> implementors = new HashSet<IImplementor>();
			implementors.Add(new FPGAImplementor(this));
			Implementors = implementors;
		}

		#endregion
		
		#region Helpers
		/// <summary>
		/// Retrieve the full path to the executable for the tool.
		/// </summary>
		public string FindToolPath(string tool)
		{
			string toolRawName = Path.GetFileNameWithoutExtension(tool);
			foreach (string path in BinaryPaths)
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
		#endregion

		public void LoadDevices(DeviceManager manager)
		{
			foreach (ToolchainReference reference in manager.CachedToolchains)
			{
				if (reference.Match(this))
				{
					// Already Loaded
					Logger.Instance.WriteDebug("DeviceManager has already cached device information for toolchain ('{0}')", this.Version);
					return;
				}
			}

			manager.CachedToolchains.Add(new ToolchainReference(this));
			// Toolchain has not been loaded
			Logger.Instance.WriteDebug("Loading device information for toolchain ('{0}')", Version);
			DeviceManufacture xilinx = manager.CreateManufacture("Xilinx");
			Logger.Instance.WriteVerbose("Loading Xilinx {0} Part Library (this may take several minutes)", Version);
			foreach (string family in XilinxPartGen.LoadFamilyList())
			{
				Logger.Instance.WriteDebug("Loading Xilinx Part for the '{0}' family", family);
				xilinx.Families.Add(XilinxPartGen.LoadFamily(this, xilinx, family));
			}

			manager.Save();
		}
	}
}
