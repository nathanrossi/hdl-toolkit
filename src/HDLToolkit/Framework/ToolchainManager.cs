using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework
{
	public class ToolchainManager
	{
		public HashSet<IToolchain> Toolchains { get; private set; }
		public DeviceManager Devices { get; private set; }

		public ToolchainManager()
		{
			Toolchains = new HashSet<IToolchain>();
			Devices = new DeviceManager();
		}

		public void AddToolchain(IToolchain toolchain)
		{
			Toolchains.Add(toolchain);
		}

		public IToolchain FindToolchainById(string id)
		{
			foreach (IToolchain toolchain in Toolchains)
			{
				if (string.Compare(toolchain.UniqueId, id, true) == 0)
				{
					return toolchain;
				}
			}
			return null;
		}
	}
}
