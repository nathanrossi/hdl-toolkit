using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Synthesis;
using HDLToolkit.Framework.Simulation;
using HDLToolkit.Framework.Implementation;

namespace HDLToolkit.Framework
{
	public interface IToolchain
	{
		ToolchainManager Manager { get; }
		IToolchainVersion Version { get; }
		string UniqueId { get; }

		IEnumerable<ISimulator> Simulators { get; }
		IEnumerable<ISynthesizer> Synthesizers { get; }
		IEnumerable<IImplementor> Implementors { get; }
	}
}
