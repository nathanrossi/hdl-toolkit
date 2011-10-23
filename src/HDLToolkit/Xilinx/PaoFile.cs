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

namespace HDLToolkit.Xilinx
{
	public static class PaoFile
	{
		public static void LoadPaoFileIntoRepository(XilinxRepository repository, string filepath, string libraryName)
		{
			List<string> librariesToExpand = new List<string>();

			librariesToExpand.AddRange(LoadPaoFileIntoRepositoryRaw(repository, filepath, libraryName));

			// Expand Libraries that need manual loading
			while (librariesToExpand.Count != 0)
			{
				string expand = librariesToExpand[0];
				librariesToExpand.RemoveAt(0);

				// Open the relevant PAO File and load in the FileElements
				string paoLocation = repository.GetLibraryPaoFile(expand);
				librariesToExpand.AddRange(LoadPaoFileIntoRepositoryRaw(repository, paoLocation, expand));
			}
		}

		private static IEnumerable<string> LoadPaoFileIntoRepositoryRaw(XilinxRepository repository, string filepath, string libraryName)
		{
			List<PaoFileModuleElement> fileElements = LoadInPaoFile(filepath);
			HashSet<string> librariesToExpand = new HashSet<string>();
			ILibrary libraryLoading = repository.GetLibraryAutoCreate(libraryName);

			// Expand File Elements
			while (fileElements.Count != 0)
			{
				PaoFileModuleElement element = fileElements[0];
				fileElements.RemoveAt(0);

				// Handles libraries outside of current pao library
				// When creating a library, the library needs to be manually loaded also, add it to the manual load collection
				// This check here ensures the library is only ever loaded once
				if (!repository.LibraryExists(element.Library) && string.Compare(libraryName, element.Library) != 0)
				{
					librariesToExpand.Add(element.Library);
				}
				ILibrary library = repository.GetLibraryAutoCreate(element.Library);

				if (element.LibraryAllReference)
				{
					// Collect Expected Libraries to Expand
					libraryLoading.AddReference(repository.GetLibraryAutoCreate(element.Library));
					if (repository.VerboseOutput)
					{
						Console.WriteLine("Library '{0}' referenced into library '{1}'", element.Library, libraryLoading.Name);
					}
				}
				else
				{
					IModule module = new GenericModule(library, element.ModuleType, element.Module);
					module.Execution = element.ExecutionType;
					library.AddModule(module);

					// Check non-specific references
					if (library != libraryLoading)
					{
						libraryLoading.AddReference(library);
						if (repository.VerboseOutput)
						{
							Console.WriteLine("Library '{0}' referenced into library '{1}'", library.Name, libraryLoading.Name);
						}
					}
					if (repository.VerboseOutput)
					{
						Console.WriteLine("Module loaded into library '{0}', with module name '{1}'", library.Name, module.RelativeLocation);
					}
				}
			}

			return librariesToExpand;
		}

		private struct PaoFileModuleElement
		{
			public string Library;
			public string Module;
			public ModuleType ModuleType;
			public ExecutionType ExecutionType;

			public bool LibraryAllReference;
		}

		private static List<PaoFileModuleElement> LoadInPaoFile(string filePath)
		{
			using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					List<PaoFileModuleElement> elements = new List<PaoFileModuleElement>();

					string line;
					int lineNumber = 0;
					while ((line = reader.ReadLine()) != null)
					{
						// Read a Line and Process it
						lineNumber++;
						line = line.Trim();
						ParsePaoFileLine(elements, line, lineNumber);
					}

					return elements;
				}
			}
		}

		private static void ParsePaoFileLine(List<PaoFileModuleElement> elements, string line, int lineNumber)
		{
			if (!string.IsNullOrEmpty(line))
			{
				if (line[0] == '#')
				{
					// Ignore this line comment
					//Console.WriteLine("COMMENT({0}): {1}", lineNumber, line);
				}
				else
				{
					// tooltarget libraryname <relative path from library's hdl dir>/filename[.v|.vhd] hdlang
					string toolTarget = null; // 'lib', 'synlib', 'simlib', 'vlgincdir'
					string libraryName = null; // specifies the libraries name
					string path = null; // specifies the path to the file (in the case of all, a recursive pao lookup must be performed)
					string hdlang = null; // 'vhdl', 'verilog'

					string[] splitUp = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
					if (splitUp.Length >= 3)
					{
						int splitPoint = 0;
						toolTarget = splitUp[splitPoint++].Trim();
						libraryName = splitUp[splitPoint++].Trim();
						path = splitUp[splitPoint++].Trim();
						if (splitUp.Length >= 4)
						{
							hdlang = splitUp[splitPoint++].Trim();
						}

						PaoFileModuleElement element = new PaoFileModuleElement();
						element.Library = libraryName;
						element.ModuleType = EnumHelpers.ParseModuleType(hdlang);
						element.ExecutionType = EnumHelpers.ParseExecutionType(toolTarget);
						element.Module = path.Trim('\"');
						if (string.Compare(element.Module, "all", true) == 0)
						{
							element.Module = null;
							element.LibraryAllReference = true;
						}

						elements.Add(element);

						//Console.WriteLine("PAO LINE({0}): tool='{1}', library='{2}', path='{3}', hdlang='{4}'", lineNumber, toolTarget, libraryName, path, hdlang);
					}
					else
					{
						throw new FileLoadException(string.Format("File is not correctly formatted on line {0}", lineNumber));
					}
				}
			}
			else
			{
				//Console.WriteLine("EMPTY({0})", lineNumber);
			}
		}
	}
}
