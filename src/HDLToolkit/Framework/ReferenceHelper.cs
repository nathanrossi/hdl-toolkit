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
using System.Text.RegularExpressions;
using System.IO;

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

		public static IModule GetVhdlModuleReferences(IModule module)
		{
			if (module.Type != ModuleType.Vhdl)
			{
				throw new Exception("Only vhdl supported");
			}

			IRepository repo = module.Environment;

			// Open the file, and find all references
			Regex referenceRegex = new Regex(@"^\s*use\s*(?<module>.*?);.*?", RegexOptions.IgnoreCase | RegexOptions.Multiline);

			List<string> references = new List<string>();

			Match matchResult = referenceRegex.Match(File.ReadAllText(module.FileLocation));
			while (matchResult.Success)
			{
				string dropLastSection = matchResult.Groups["module"].Value;
				dropLastSection = dropLastSection.Substring(0, dropLastSection.LastIndexOf('.'));
				references.Add(dropLastSection);
				

				matchResult = matchResult.NextMatch();
			}

			module.ModuleReferences.Clear();

			foreach (string s in references)
			{
				string[] split = s.Split('.');

				if (split.Length == 2)
				{
					string moduleName = split[1].ToLower();
					string libraryName = split[0].ToLower();

					if (string.Compare(libraryName, "ieee") != 0 && string.Compare(libraryName, "unisim") != 0)
					{
						ILibrary library = repo.GetLibrary(libraryName);

						foreach (IModule m in library.Modules)
						{
							if (string.Compare(m.Name, moduleName, true) == 0)
							{
								module.ModuleReferences.Add(m);
								break;
							}
						}
					}
				}
			}

			Logger.Instance.WriteDebug("module: {0}", module.Name);
			foreach (IModule m in module.ModuleReferences)
			{
				Logger.Instance.WriteDebug("\t > references {0}.{1}", m.Parent.Name, m.Name);
			}

			return module;
		}

		public static List<IModule> SortModulesByReference(IEnumerable<IModule> modules)
		{
			List<IModule> allModules = new List<IModule>(GetAllModules(modules));

			foreach (IModule m in allModules)
			{
				GetVhdlModuleReferences(m);
			}

			// Note: Cannot handle circular references!
			// first element must be fisrt in reference tree
			List<IModule> trueList = new List<IModule>();

			while (allModules.Count != 0)
			{
				IModule top = allModules.First();
				allModules.Remove(top);
				// where to put it in sortList?
				if (trueList.Count == 0)
				{
					trueList.Add(top);
				}
				else
				{
					// check if any of the referenced modules are in the trueList
					bool inserted = false;
					for (int i = trueList.Count - 1; i >= 0; i--)
					{
						// if the element in trueList is a reference for the inserting module
						// it must be further up in the chain
						//top.ModuleReferences.Contains(trueList[i]);

						// if the inserting module is a reference of the element in the trueList
						// it must be here or further down in the chain, insert it here
						if (trueList[i].ModuleReferences.Contains(top))
						{
							trueList.Insert(i + 1, top);
							inserted = true;
							break;
						}
					}

					if (!inserted)
					{
						trueList.Insert(0, top);
						inserted = true;
					}
				}
			}

			return trueList;
		}
	}
}
