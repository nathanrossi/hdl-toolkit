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

namespace HDLToolkit.Framework
{
	public enum ModuleType
	{
		Unknown,
		Vhdl,
		Verilog
	}

	public enum ExecutionType
	{
		All,
		NotSupported,
		SynthesisOnly,
		SimulationOnly
	}

	public static class EnumHelpers
	{
		public static string ModuleTypeToString(ModuleType type)
		{
			if (type == ModuleType.Vhdl)
			{
				return "vhdl";
			}
			else if (type == ModuleType.Verilog)
			{
				return "verilog";
			}
			return "unknown";
		}

		public static ModuleType ParseModuleType(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				string lowerCase = str.ToLower();
				if (lowerCase.CompareTo("vhdl") == 0)
				{
					return ModuleType.Vhdl;
				}
				else if (lowerCase.CompareTo("verilog") == 0)
				{
					return ModuleType.Verilog;
				}
			}
			return ModuleType.Unknown;
		}

		public static ExecutionType ParseExecutionType(string str)
		{
			if (!string.IsNullOrEmpty(str))
			{
				string lowerCase = str.ToLower();
				if (lowerCase.CompareTo("lib") == 0)
				{
					return ExecutionType.All;
				}
				else if (lowerCase.CompareTo("simlib") == 0)
				{
					return ExecutionType.SimulationOnly;
				}
				else if (lowerCase.CompareTo("synlib") == 0)
				{
					return ExecutionType.SynthesisOnly;
				}
				else if (lowerCase.CompareTo("vlgincdir") == 0) // Not Supported
				{
					throw new NotSupportedException("vlgincdir - Verilog Include Directory Not Supported");
				}
			}
			return ExecutionType.NotSupported;
		}

		public static bool ExecutionTypeMatchesRequirement(ExecutionType requirement, ExecutionType current)
		{
			if (requirement == ExecutionType.All)
			{
				return true;
			}
			else if (requirement == ExecutionType.SimulationOnly && current == ExecutionType.SimulationOnly || current == ExecutionType.All)
			{
				return true;
			}
			else if (requirement == ExecutionType.SynthesisOnly && current == ExecutionType.SynthesisOnly || current == ExecutionType.All)
			{
				return true;
			}
			return false;
		}
	}
}
