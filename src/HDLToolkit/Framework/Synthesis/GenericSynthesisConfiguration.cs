using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Synthesis
{
	public class GenericSynthesisConfiguration : ISynthesisConfiguration
	{
		public IModule Module { get; set; }
		public DevicePartSpeed TargetDevice { get; set; }
	}
}
