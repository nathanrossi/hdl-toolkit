using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit
{
	public class StringProcessListener : IProcessListener
	{
		private StringBuilder builderOut;
		private StringBuilder builderErr;

		public string Output { get { return builderOut.ToString(); } }
		public string ErrorOutput { get { return builderErr.ToString(); } }

		public StringProcessListener()
		{
			builderOut = new StringBuilder();
			builderErr = new StringBuilder();
		}

		public void ProcessLine(string line)
		{
			builderOut.AppendLine(line);
		}

		public void ProcessErrorLine(string line)
		{
			builderErr.AppendLine(line);
		}

		public void Clear()
		{
			builderOut = new StringBuilder();
			builderErr = new StringBuilder();
		}

		public void Dispose()
		{
			// Nothing to do here
		}
	}
}
