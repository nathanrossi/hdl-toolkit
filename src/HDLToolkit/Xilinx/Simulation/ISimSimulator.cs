using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
			string result = InjectCommand("show time");

			TimeUnit value;
			if (TimeUnit.TryParse(result, out value))
			{
				return value;
			}
			throw new Exception("Unable to parse ISim output.");
		}

		public object GetSignalState(string path)
		{
			CheckRunning();
			string result = InjectCommand("show value " + path);

			if (result.EndsWith("No such HDL Object\n"))
			{
				throw new Exception("Object on path does not exist");
			}

			object output = ParseSignalOutput(result);
			if (output != null)
			{
				return output;
			}
			throw new Exception("Unable to parse ISim output.");
		}

		private object ParseSignalOutput(string output)
		{
			if (output.Contains('(') || output.Contains(')'))
			{
				throw new NotImplementedException("Arrays, and record types are not supported. To get the value index the object manually.");
			}

			// Is it a slv output?
			StdLogicVector slv = StdLogicVector.Parse(output);
			if (slv != null)
			{
				return slv;
			}

			// Is it a boolean output?
			bool boolean = false;
			if (bool.TryParse(output, out boolean))
			{
				return boolean;
			}

			// Is it an integer output?
			int integer = 0;
			if (int.TryParse(output, out integer))
			{
				return integer;
			}

			// No idea
			return null;
		}
	}
}
