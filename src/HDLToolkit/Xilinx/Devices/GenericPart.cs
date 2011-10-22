using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Devices
{
	public class GenericPart : IPart
	{
		public string Name { get; set; }
		public IPartFamily Parent { get; private set; }

		public IList<IPartDevice> Devices { get; private set; }

		public GenericPart(IPartFamily family, string name)
		{
			Parent = family;
			Name = name;

			Devices = new List<IPartDevice>();
		}

		public GenericPartDevice CreateDevice(IPartPackage package)
		{
			GenericPartDevice device = new GenericPartDevice(this, package);
			Devices.Add(device);
			return device;
		}
	}
}
