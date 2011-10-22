using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Devices
{
	public interface IPartPackage
	{
		string Name { get; }
		IPartFamily Parent { get; } // the parent part IPart
	}
}
