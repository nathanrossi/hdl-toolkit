using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ISAGenericTestSuiteRunner
{
	public abstract class TestCommand
	{
		public string Parameters { get; set; }

		public int Address { get; set; }
		public int CyclesAfterEvent { get; set; }

		public TestCommand(int addr, int cycles, string parameters)
		{
			Address = addr;
			CyclesAfterEvent = cycles;
			Parameters = parameters;
		}

		public virtual void Execute(TestBench test, ProcessorState state)
		{

		}
	}
}
