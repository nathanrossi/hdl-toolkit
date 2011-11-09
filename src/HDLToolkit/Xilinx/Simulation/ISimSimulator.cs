using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Simulation;

namespace HDLToolkit.Xilinx.Simulation
{
	public class ISimSimulator : ISimProcess, ISimulator
	{
		public ISimSimulator(string workingDirectory, string executable)
			: base(workingDirectory, executable)
		{

		}

		private void CheckRunning()
		{
			if (!Running)
			{
				throw new Exception("Simulator must be running to execution commands.");
			}
		}

		public void RunFor(long nanoseconds)
		{
			CheckRunning();
			// Generate the command string
			string command = string.Format("run {0} ns", nanoseconds);
			InjectCommand(command);
		}

		public void Restart()
		{
			CheckRunning();
			InjectCommand("restart");
		}

		public TimeUnit GetCurrentTime()
		{
			CheckRunning();
			string result = InjectCommand("puts [show time]"); // wrap with puts output to terminal

			TimeUnit value;
			if (TimeUnit.TryParse(TrimExcessData(result), out value))
			{
				return value;
			}
			throw new Exception("Unable to parse ISim output.");
		}

		public StdLogicVector GetSignalState(string path)
		{
			CheckRunning();
			string result = InjectCommand("puts [show value " + path + " -radix bin]");

			if (result.EndsWith("No such HDL Object\n"))
			{
				throw new Exception("Object on path does not exist");
			}

			StdLogicVector output = ParseSignalOutput(result);
			if (output != null)
			{
				return output;
			}
			throw new Exception("Unable to parse ISim output.");
		}

		private static string TrimExcessData(string str)
		{
			return str.Trim(' ', '\r', '\n');
		}

		private StdLogicVector ParseSignalOutput(string output)
		{
			string val = TrimExcessData(output);
			if (val.Contains('(') || val.Contains(')'))
			{
				throw new NotImplementedException("Arrays, and record types are not supported. To get the value index the object manually.");
			}

			// Is it a slv output?
			StdLogicVector slv = StdLogicVector.Parse(val);
			if (slv != null)
			{
				return slv;
			}

			// Is it a boolean output?
			bool boolean = false;
			if (bool.TryParse(val, out boolean))
			{
				return new StdLogicVector(new bool[] { boolean });
			}

			// No idea
			return null;
		}
	}
}
