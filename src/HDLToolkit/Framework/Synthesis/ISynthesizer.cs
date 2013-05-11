using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Framework.Synthesis
{
	public interface ISynthesizer
	{
		IToolchain Toolchain { get; }

		// Configuration Options
		// ...

		ISynthesizerInstance Create(OutputPath output, ISynthesisConfiguration configuration);
	}
}
