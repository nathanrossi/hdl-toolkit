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
	public static class ReferenceHelper
	{
		public static IEnumerable<IModule> GetAllModules(IEnumerable<IModule> modules)
		{
			HashSet<IModule> allModules = new HashSet<IModule>();
			HashSet<ILibrary> expandLibraries = new HashSet<ILibrary>();
			HashSet<ILibrary> expandedLibraries = new HashSet<ILibrary>();

			foreach (IModule module in modules)
			{
				expandLibraries.Add(module.Parent);
			}

			// expand Libraries
			while (expandLibraries.Count != 0)
			{
				ILibrary top = expandLibraries.First();
				expandLibraries.Remove(top);

				if (expandedLibraries.Add(top))
				{
					foreach (ILibrary reference in top.References)
					{
						expandLibraries.Add(reference);
					}
				}
			}

			// Expand all libraries back to Modules
			foreach (ILibrary library in expandedLibraries)
			{
				foreach (IModule module in library.Modules)
				{
					allModules.Add(module);
				}
			}

			return allModules;
		}
	}
}
