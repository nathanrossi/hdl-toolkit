using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Devices
{
	public class GenericPartSpeed : IPartSpeed
	{
		public string Name { get; private set; }
		public IPartFamily Parent { get; private set; }

		public GenericPartSpeed(IPartFamily family, string name)
		{
			Parent = family;
			Name = name;
		}
	}
}
