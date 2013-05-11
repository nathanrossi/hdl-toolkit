using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Implementation
{
	public class GenericImplementationConfiguration : IImplementationConfiguration
	{
		public string NetList { get; set; }
		public string Constraints { get; set; }
		public DevicePartSpeed TargetDevice { get; set; }
	}
}
