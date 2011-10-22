using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Devices
{
	public class GenericPartDevice : IPartDevice
	{
		public string Name
		{
			get { return Parent.Name + Package.Name; }
		}
		public IPart Parent { get; private set; }

		public IPartPackage Package { get; private set; }
		public IList<IPartSpeed> Speeds { get; private set; }

		public GenericPartDevice(IPart part, IPartPackage package)
		{
			Parent = part;
			Package = package;

			Speeds = new List<IPartSpeed>();
		}
	}
}
