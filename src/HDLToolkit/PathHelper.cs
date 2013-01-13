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
using System.IO;
using HDLToolkit.Framework;

namespace HDLToolkit
{
	public static class PathHelper
	{
		public static string Combine(params string[] args)
		{
			if (args.Length >= 1)
			{
				string complete = args[0];

				if (args.Length >= 2)
				{
					for (int i = 1; i < args.Length; i++)
					{
						complete = Path.Combine(complete, args[i]);
					}
				}

				return complete;
			}
			return null;
		}

		public static string GetFullPath(string path)
		{
			if (path == null)
			{
				return null;
			}
			else
			{
				return Path.GetFullPath(path);
			}
		}

		public static string AddOmittedExtensionToFile(string file, ModuleType type)
		{
			if (string.IsNullOrEmpty(Path.GetExtension(file)))
			{
				if (type == ModuleType.Vhdl)
				{
					return Path.ChangeExtension(file, "vhd");
				}
				else if (type == ModuleType.Verilog)
				{
					return Path.ChangeExtension(file, "v");
				}
				else
				{
					throw new NotSupportedException("Unsupported ModuleType");
				}
			}
			return file;
		}

		public static string StripFileExtension(string file)
		{
			return Path.ChangeExtension(file, "").TrimEnd('.');
		}
	}
}
