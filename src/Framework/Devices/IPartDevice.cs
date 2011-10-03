using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Devices
{
	public interface IPartDevice
	{
		string Name { get; }
		IPart Parent { get; }

		IPartPackage Package { get; }
		IList<IPartSpeed> Speeds { get; }
	}
}
