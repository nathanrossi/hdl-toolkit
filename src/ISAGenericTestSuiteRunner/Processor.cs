using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Xilinx.Simulation;

namespace ISAGenericTestSuiteRunner
{
	public class Processor
	{
		public ISimSimulator Simulator { get; set; }

		public Processor(ISimSimulator sim)
		{
			Simulator = sim;
		}

		public ProcessorState GetCurrentState()
		{
			ProcessorState state = new ProcessorState();

			state.Pipeline = new ProcessorState.ProgramCounterState[2];

			state.Pipeline[0].Valid = ((Simulator.GetSignalState("UUT/instr_valid(1)").ToLong()) > 0); // current Valid
			state.Pipeline[0].Value = (int)(Simulator.GetSignalState("UUT/pcs(1)").Flip().ToLong()); // current PC

			state.Pipeline[1].Valid = ((Simulator.GetSignalState("UUT/instr_valid(0)").ToLong()) > 0); // next Valid
			state.Pipeline[1].Value = (int)(Simulator.GetSignalState("UUT/pcs(0)").Flip().ToLong()); // next PC

			state.PipelineBusy = !state.Pipeline[0].Valid; // pipeline busy fetching a valid instruction

			state.StatusRegister = (int)(Simulator.GetSignalState("UUT/state_1.rs(0)").Flip().ToLong()); // status register

			state.Registers = new int[32];
			for (int i = 0; i < 32; i++)
			{
				state.Registers[i] = GetRegister(i);
			}

			return state;
		}

		public int GetRegister(int index)
		{
			if (index <= 31)
			{
				return (int)(Simulator.GetSignalState("UUT/gprf/ISO_REG_FILE_INST/ram(" + index.ToString() + ")(7:0)").Flip().ToLong());
			}
			throw new ArgumentOutOfRangeException("Only 32 registers (0-31)");
		}

		public void RunCycle()
		{
			Simulator.RunFor(10);
		}

		public void RunToNextValidInstruction()
		{
			while (true)
			{
				bool nextValid = ((Simulator.GetSignalState("UUT/instr_valid(0)").ToLong()) > 0);

				if (nextValid)
				{
					return;
				}

				RunCycle();
			}
		}
	}
}
