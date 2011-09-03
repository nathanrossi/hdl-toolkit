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
	public static class SystemHelper
	{
		public static string EnvironmentPathSeperator
		{
			get
			{
				if (GetSystemType() == SystemType.Windows)
				{
					return ";";
				}
				else
				{
					return ":";
				}
			}
		}

		// This is an enum specific to Xilinx/etc not to the .NET Platform
		public enum SystemType
		{
			Unknown,
			Windows,
			Linux
		}

		public static SystemType GetSystemType()
		{
			int id = (int)Environment.OSVersion.Platform;
			if (id == 4 || id == 6 || id == 128)
			{
				return SystemType.Linux;
			}
			else
			{
				return SystemType.Windows;
			}
		}

		#region Environment_Helpers

		public static string EnvironmentPathAppend(string path, string value)
		{
			if (string.IsNullOrEmpty(path))
			{
				return value;
			}
			else
			{
				return string.Format("{0}{1}{2}", path, EnvironmentPathSeperator, value);
			}
		}

		public static string EnvironmentPathAppend(string path, List<string> values)
		{
			string result = path;
			foreach (string s in values)
			{
				result = EnvironmentPathAppend(result, s);
			}
			return result;
		}

		public static string EnvironmentPathPrepend(string path, string value)
		{
			if (string.IsNullOrEmpty(path))
			{
				return value;
			}
			else
			{
				return string.Format("{0}{1}{2}", value, EnvironmentPathSeperator, path);
			}
		}

		public static string EnvironmentPathPrepend(string path, List<string> values)
		{
			string result = path;
			foreach (string s in values)
			{
				result = EnvironmentPathPrepend(result, s);
			}
			return result;
		}

		#endregion
	}
}
