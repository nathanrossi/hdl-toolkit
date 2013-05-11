using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Implementation
{
	public interface IImplementor
	{
		IToolchain Toolchain { get; }
		
		// Configuration Options
		// ...

		IImplementorInstance Create(OutputPath output, IImplementationConfiguration configuration);
	}
}
