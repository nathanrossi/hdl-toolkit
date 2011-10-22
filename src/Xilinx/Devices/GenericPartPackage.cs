using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Devices
{
	public class GenericPartPackage : IPartPackage
	{
		public string Name { get; private set; }
		public IPartFamily Parent { get; private set; }

		public GenericPartPackage(IPartFamily family, string name)
		{
			Parent = family;
			Name = name;
		}
	}
}
