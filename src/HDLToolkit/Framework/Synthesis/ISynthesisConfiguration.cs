using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Synthesis
{
	public interface ISynthesisConfiguration
	{
		IModule Module { get; }
		DevicePartSpeed TargetDevice { get; }
	}
}
