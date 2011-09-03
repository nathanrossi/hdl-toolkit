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

namespace HDLToolkit.Framework
{
	public interface ILibrary
	{
		/// <summary>
		/// All modules inside the library.
		/// </summary>
		ICollection<IModule> Modules { get; }
		/// <summary>
		/// All reference to other library for this library
		/// </summary>
		ICollection<ILibrary> References { get; }

		/// <summary>
		/// The name of the library.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The environment that contains this library.
		/// </summary>
		IRepository Environment { get; }

		/// <summary>
		/// Add a module to the library. Returns the Unique Module that represents the module. (No Duplicates)
		/// </summary>
		IModule AddModule(IModule module);
		/// <summary>
		/// Add a reference to another library. Returns the Unique Library that represents the module. (No Duplicates)
		/// </summary>
		ILibrary AddReference(ILibrary library);

		/// <summary>
		/// Parses the entire tree for all modules that this library depends on.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IModule> GetAllReferencedModules();
	}
}
