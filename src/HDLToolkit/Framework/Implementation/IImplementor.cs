using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Implementation
{
	interface IImplementor : IDisposable
	{
		string NetList { get; }
		DevicePartSpeed TargetDevice { get; }

		OutputPath OutputLocation { get; }
		List<string> Artifacts { get; }

		Dictionary<string, string> Configuration { get; }

		/// <summary>
		/// Begin the build process.
		/// </summary>
		/// <returns>Return true on successful build, false otherwise.</returns>
		bool Build();
	}
}
