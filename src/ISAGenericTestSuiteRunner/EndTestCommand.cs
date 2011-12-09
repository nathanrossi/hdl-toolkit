using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISAGenericTestSuiteRunner
{
	public class EndTestCommand : TestCommand
	{
		public EndTestCommand(int addr, int cycles, string parameters)
			: base(addr, cycles, parameters)
		{
		}

		public override void Execute(TestBench test, ProcessorState state)
		{
			test.EndTest();
		}
	}
}
