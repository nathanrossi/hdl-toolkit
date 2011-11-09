using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Framework.Simulation
{
	public struct TimeUnit
	{
		private long nanoseconds;

		public TimeUnit(long nanoseconds)
		{
			this.nanoseconds = nanoseconds;
		}

		public static bool TryParse(string parse, out TimeUnit value)
		{
			value.nanoseconds = 0;
			string[] values = parse.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (values.Length == 2)
			{
				long number = 0;
				if (long.TryParse(values[0], out number))
				{
					// Number parsed correctly
					if (string.Compare(values[1], "fs", true) == 0)
					{
						value.nanoseconds = number / 1000000;
					}
					else if (string.Compare(values[1], "ps", true) == 0)
					{
						value.nanoseconds = number / 1000;
					}
					else if (string.Compare(values[1], "ns", true) == 0)
					{
						value.nanoseconds = number;
					}
					else if (string.Compare(values[1], "us", true) == 0)
					{
						value.nanoseconds = number * 1000;
					}
					else if (string.Compare(values[1], "ms", true) == 0)
					{
						value.nanoseconds = number * 1000000;
					}
					else if (string.Compare(values[1], "s", true) == 0)
					{
						value.nanoseconds = number * 1000000000;
					}
					else
					{
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0} ns", nanoseconds);
		}
	}
}
