using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Devices
{
	public interface IPart
	{
		string Name { get; }
		IPartFamily Parent { get; }

		IList<IPartDevice> Devices { get; }
	}
}
