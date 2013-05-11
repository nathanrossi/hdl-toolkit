using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework
{
	public interface IToolchainVersion
	{
		string UniqueId { get; }

		string ToString();
	}
}
