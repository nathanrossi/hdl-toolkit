using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Xilinx.Implementation.BlockRAM
{
	public static class MemFormatHelper
	{
		public static string ConvertBinaryToMem(byte[] data)
		{
			StringBuilder builder = new StringBuilder();

			// Append adress at 0 header
			builder.AppendFormat("@0000");

			for (int i = 0; i < data.Length; i++)
			{
				builder.AppendFormat(" {0:X2}", data[i]);
			}

			return builder.ToString();
		}
	}
}
