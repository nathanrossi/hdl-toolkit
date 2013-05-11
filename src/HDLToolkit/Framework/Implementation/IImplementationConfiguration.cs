using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Implementation
{
	public interface IImplementationConfiguration
	{
		string NetList { get; }
		string Constraints { get; }
		DevicePartSpeed TargetDevice { get; }
	}
}
