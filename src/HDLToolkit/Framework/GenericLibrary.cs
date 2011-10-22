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
	public class GenericLibrary : ILibrary
	{
		public IRepository Environment { get; private set; }
		public ICollection<IModule> Modules { get; private set; }
		public ICollection<ILibrary> References { get; private set; }
		public string Name { get; private set; }

		public GenericLibrary(IRepository environment, string name)
		{
			Environment = environment;
			Modules = new HashSet<IModule>();
			References = new HashSet<ILibrary>();
			Name = name;
		}

		public IModule AddModule(IModule module)
		{
			// Add
			Modules.Add(module);

			return module;
		}

		public ILibrary AddReference(ILibrary library)
		{
			// Add
			References.Add(library);

			return library;
		}

		public IEnumerable<IModule> GetAllReferencedModules()
		{
			HashSet<IModule> set = new HashSet<IModule>();

			foreach (ILibrary library in References)
			{
				foreach (IModule module in library.Modules)
				{
					set.Add(module);
				}
			}

			foreach (IModule module in Modules)
			{
				set.Add(module);
			}

			return set;
		}

		public override string ToString()
		{
			return string.Format("{0}", Name);
		}
	}
}
