using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Xilinx.Simulation;

namespace ISAGenericTestSuiteRunner
{
	public class ProcessorState
	{
		public struct ProgramCounterState
		{
			public int Value { get; set; }
			public bool Valid { get; set; }
		}

		public ProgramCounterState[] Pipeline { get; set; }
		public bool PipelineStalled { get; set; }

		public int[] Registers { get; set; }
		public int StatusRegister { get; set; }

		public ProcessorState()
		{
		}
	}
}
