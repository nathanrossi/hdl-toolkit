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
	public interface IModule
	{
		/// <summary>
		/// The type of the module, eg VHDL or Verilog
		/// </summary>
		ModuleType Type { get; set; }
		/// <summary>
		/// The execution type, All or syn/sim.
		/// </summary>
		ExecutionType Execution { get; set; }

		/// <summary>
		/// The environment that contains this module.
		/// </summary>
		IRepository Environment { get; }
		/// <summary>
		/// The parent library, the owner of the module.
		/// </summary>
		ILibrary Parent { get; }

		/// <summary>
		/// The name of the module.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The relative location of the Module to the Library. (the absolute location is defined by the IRepository)
		/// </summary>
		string RelativeLocation { get; set; }
		/// <summary>
		/// The location of the Module, an absolute path to the file.
		/// </summary>
		string FileLocation { get; }
	}
}
