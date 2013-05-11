using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Synthesis
{
	public interface ISynthesizerInstance : IDisposable
	{
		ISynthesizer Synthesizer { get; }
		OutputPath OutputLocation { get; }
		ISynthesisConfiguration Configuration { get; }

		/// <summary>
		/// Begin the build process.
		/// </summary>
		/// <returns>Return true on successful build, false otherwise.</returns>
		bool Build();
	}
}
