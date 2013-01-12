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
using System.Text.RegularExpressions;

namespace HDLToolkit.Xilinx
{
	public class XilinxRepository : IRepository
	{
		private static Regex libraryNameRegex = new Regex(@"(?<name>.*)_(?<version>v\d*_\d*)_(?<tag>.*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

		public bool VerboseOutput = false;

		private List<string> SearchPaths { get; set; }
		private List<string> expandedPCorePaths = null;
		public List<ILibrary> Libraries { get; protected set; }

		public XilinxRepository()
		{
			SearchPaths = new List<string>();
			Libraries = new List<ILibrary>();
		}

		public ILibrary GetLibrary(string name)
		{
			// Search the loaded collection
			foreach (ILibrary library in Libraries)
			{
				if (library.Name.CompareTo(name) == 0)
				{
					return library;
				}
			}
			
			// Library not found in collection, attempt to load it
			if (LoadLibrary(name))
			{
				// Library is loaded, and use a recursive call to get it.
				return GetLibrary(name);
			}

			throw new Exception("Library " + name + " does not exist within repository.");
		}

		public bool LibraryExists(string name)
		{
			foreach (ILibrary library in Libraries)
			{
				if (library.Name.CompareTo(name) == 0)
				{
					return true;
				}
			}
			return false;
		}

		internal ILibrary GetLibraryAutoCreate(string name)
		{
			// Search the loaded collection
			foreach (ILibrary library in Libraries)
			{
				if (library.Name.CompareTo(name) == 0)
				{
					return library;
				}
			}

			// Library not found in collection, attempt to load it
			ILibrary newLibrary = new GenericLibrary(this, name);
			Libraries.Add(newLibrary);
			return newLibrary;
		}

		private bool LoadLibrary(string name)
		{
			string paoFile = GetLibraryPaoFile(name);
			if (!string.IsNullOrEmpty(paoFile))
			{
				PaoFile.LoadPaoFileIntoRepository(this, paoFile, name);
				return true;
			}
			return false;
		}

		public List<string> GetPcoreDirectories()
		{
			// Check if cache is valid
			if (expandedPCorePaths != null)
			{
				return expandedPCorePaths;
			}

			List<string> directories = new List<string>();

			foreach (string repo in SearchPaths)
			{
				int countStart = directories.Count;
				IEnumerable<string> toAdd = Directory.GetDirectories(repo, "pcores", SearchOption.TopDirectoryOnly);
					foreach (string add in toAdd)
					{
						if (!directories.Contains(add))
						{
							directories.Add(add);
						}
					}
				// Enumerate at depth of 2
				foreach (string subDirectory in Directory.GetDirectories(repo, "*"))
				{
					IEnumerable<string> toAddSub = Directory.GetDirectories(subDirectory, "pcores", SearchOption.TopDirectoryOnly);
					foreach (string addSub in toAddSub)
					{
						if (!directories.Contains(addSub))
						{
							directories.Add(addSub);
						}
					}
				}

				//Console.WriteLine("For {0}, {1} pcore directories were found", repo, directories.Count - countStart);
				//foreach (string pcoreDirectory in directories)
				//{
				//    Console.WriteLine("\t{0}", pcoreDirectory);
				//}
			}

			// Cache the directories
			expandedPCorePaths = directories;
			return directories;
		}

		public List<string> GetLibraryRootPath(string library)
		{
			List<string> possiblePaths = new List<string>();

			foreach (string pcoreDirectory in GetPcoreDirectories())
			{
				// Find the pcore directories
				string path = PathHelper.Combine(pcoreDirectory, library);

				if (Directory.Exists(path))
				{
					possiblePaths.Add(path);
					//Console.WriteLine("For {0}, library directory found: {1}", library, pcoreDirectory);
				}
			}

			return possiblePaths;
		}

		public string GetLibraryDefaultRootPath(string library)
		{
			return GetLibraryRootPath(library).FirstOrDefault();
		}

		private string GetLibraryNameWithoutVersion(string library)
		{
			Match m = libraryNameRegex.Match(library);
			if (m.Success)
			{
				if (m.Groups["name"] != null)
				{
					return m.Groups["name"].Value;
				}
			}
			return library;
		}

		public string GetLibraryPaoFile(string library)
		{
			string root = GetLibraryDefaultRootPath(library);
			if (root != null)
			{
				string paoFolder = PathHelper.Combine(root, "data");
				string name = GetLibraryNameWithoutVersion(library);
				string expectedPaoFile = Path.Combine(paoFolder, name + "_v2_1_0.pao");

				// Check for the expected file
				if (File.Exists(expectedPaoFile))
				{
					return expectedPaoFile;
				}

				// Search and pick first .pao file
				IEnumerable<string> paoFiles = Directory.GetFiles(paoFolder, "*.pao", SearchOption.TopDirectoryOnly);
				return paoFiles.FirstOrDefault();
			}
			return null;
		}

		public string GetModulePath(ILibrary library, string location, ModuleType type)
		{
			string libraryRoot = GetLibraryDefaultRootPath(library.Name);
			if (type == ModuleType.Vhdl)
			{
				return PathHelper.Combine(libraryRoot, "hdl", "vhdl", location);
			}
			else if (type == ModuleType.Verilog)
			{
				return PathHelper.Combine(libraryRoot, "hdl", "verilog", location);
			}
			throw new NotSupportedException();
		}

		public void AddSearchPath(string path)
		{
			// Invalidate the cache
			expandedPCorePaths = null;

			// Add the search path
			SearchPaths.Add(path);
		}

		/// <summary>
		/// Find a Module by name.
		/// </summary>
		/// <param name="name">Name of Module, e.g. "library.module".</param>
		/// <returns>The Module if found</returns>
		public IModule FindModuleByName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			string[] splitModule = name.Trim().Split('.');
			ILibrary library = GetLibrary(splitModule[0]);
			if (library != null)
			{
				IModule module = library.Modules.First((m) => string.Compare(m.Name, splitModule[1], true) == 0);
				if (module != null)
				{
					return module;
				}
			}
			return null;
		}
	}
}
