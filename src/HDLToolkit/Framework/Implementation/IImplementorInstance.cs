using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Implementation
{
	public interface IImplementorInstance : IDisposable
	{
		IImplementor Implementor { get; }
		OutputPath OutputLocation { get; }
		IImplementationConfiguration Configuration { get; }

		

		/// <summary>
		/// Begin the build process.
		/// </summary>
		/// <returns>Return true on successful build, false otherwise.</returns>
		bool Build();
	}
}
