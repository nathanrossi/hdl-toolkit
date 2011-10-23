using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HDLToolkit.Xilinx.Simulation
{
	public interface ISimulator
	{
		void RunFor(long nanoseconds);

		object GetSignalState(string path);
	}
}
