using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HDLToolkit.Framework.Simulation
{
	public interface ISimulator
	{
		void RunFor(long nanoseconds);

		StdLogicVector GetSignalState(string path);
	}
}
