using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Devices
{
	public interface IPartSpeed
	{
		string Name { get; }
		IPartFamily Parent { get; }
	}
}
