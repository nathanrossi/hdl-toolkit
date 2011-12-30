using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit
{
	public interface IProcessListener : IDisposable
	{
		void ProcessLine(string line);
		void ProcessErrorLine(string line);
	}
}
