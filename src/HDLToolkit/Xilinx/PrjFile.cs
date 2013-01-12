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
using HDLToolkit.Framework;

namespace HDLToolkit.Xilinx
{
	public class PrjFile : IProjectFile
	{
		public IRepository Environment { get; private set; }
		public ICollection<IModule> Modules { get; private set; }

		public PrjFile(IRepository repository)
		{
			Modules = new HashSet<IModule>();
			Environment = repository;
		}

		public IModule AddModule(IModule module)
		{
			Modules.Add(module);
			return module;
		}

		/// <summary>
		/// Adds all modules from a specified library.
		/// </summary>
		/// <param name="library">The specified library.</param>
		/// <returns>The library provided.</returns>
		public ILibrary AddAllInLibrary(ILibrary library)
		{
			foreach (IModule module in library.Modules)
			{
				AddModule(module);
			}
			return library;
		}

		public override string ToString()
		{
			return this.ToString(ExecutionType.All);
		}

		public string ToString(ExecutionType execution)
		{
			StringBuilder builder = new StringBuilder();

			List<IModule> modules = ReferenceHelper.SortModulesByReference(Modules);
			for (int i = modules.Count - 1; i >= 0; i--)
			{
				if (EnumHelpers.ExecutionTypeMatchesRequirement(execution, modules[i].Execution))
				{
					builder.AppendLine(IModuleToPrjLine(modules[i]));
				}
			}

			return builder.ToString();
		}

		private static string IModuleToPrjLine(IModule module)
		{
			string type = null;
			if (module.Type == ModuleType.Vhdl)
			{
				type = "vhdl";
			}
			else if (module.Type == ModuleType.Verilog)
			{
				type = "verilog";
			}

			if (type != null)
			{
				return string.Format("{0} {1} \"{2}\"", type, module.Parent.Name, module.FileLocation);
			}
			return "";
		}

		/// <summary>
		/// Create a PrjFile from a single IModule object, and expand all dependencies
		/// </summary>
		/// <param name="module">Specified module object.</param>
		/// <returns>Created PrjFile.</returns>
		public static PrjFile CreateFromIModule(IModule module)
		{
			if (module == null)
			{
				return null;
			}

			// Create the PrjFile
			PrjFile project = new PrjFile(module.Environment);
			project.AddAllInLibrary(module.Parent);

			// Scan and add dependencies
			if (module.Parent != null)
			{
				foreach (ILibrary library in module.Parent.References)
				{
					project.AddAllInLibrary(library);
				}
			}

			return project;
		}
	}
}
