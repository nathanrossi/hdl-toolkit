// Copyright 2011 Nathan Rossi - http://nathanrossi.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit
{
	public class Logger
	{
		public enum Verbosity
		{
			Off = 0,
			Low,
			Medium,
			High,
			Debug
		}

		private static Logger instance;
		public static Logger Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Logger();
				}
				return instance;
			}
		}

		public Verbosity VerbosityLevel { get; set; }

		public Logger()
		{
			VerbosityLevel = Verbosity.Off;
		}

		public void WriteInfo(object obj)
		{
			Console.WriteLine(obj);
		}

		public void WriteInfo(string format, params object[] obj)
		{
			Console.WriteLine(format, obj);
		}

		public void WriteDebug(object obj)
		{
			using (new ConsoleColorScope(ConsoleColor.Green, ConsoleColor.Black))
			{
				WriteVerbose(Verbosity.Debug, "Debug: {0}", obj);
			}
		}

		public void WriteDebug(string format, params object[] obj)
		{
			using (new ConsoleColorScope(ConsoleColor.Green, ConsoleColor.Black))
			{
				WriteVerbose(Verbosity.Debug, "Debug: " + format, obj);
			}
		}

		/// <summary>
		/// Writes with the default verbose level of low.
		/// </summary>
		public void WriteVerbose(object obj)
		{
			WriteVerbose(Verbosity.Low, obj);
		}

		/// <summary>
		/// Writes with the default verbose level of low.
		/// </summary>
		public void WriteVerbose(string format, params object[] obj)
		{
			WriteVerbose(Verbosity.Low, format, obj);
		}

		public void WriteVerbose(Verbosity level, object obj)
		{
			if (level <= VerbosityLevel)
			{
				Console.WriteLine(obj);
			}
		}

		public void WriteVerbose(Verbosity level, string format, params object[] obj)
		{
			if (level <= VerbosityLevel)
			{
				Console.WriteLine(format, obj);
			}
		}

		public void WriteWarning(string format, params object[] obj)
		{
			using (new ConsoleColorScope(ConsoleColor.Yellow, ConsoleColor.Black))
			{
				Console.WriteLine("Warning: " + format, obj);
			}
		}

		public void WriteError(string format, params object[] obj)
		{
			using (new ConsoleColorScope(ConsoleColor.Red, ConsoleColor.White))
			{
				Console.WriteLine("Error: " + format, obj);
			}
		}
	}
}
