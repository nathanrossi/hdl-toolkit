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

namespace HDLToolkit.Framework
{
	public class GenericModule : IModule
	{
		public ILibrary Parent { get; private set; }
		public IRepository Environment { get { return Parent != null ? Parent.Environment : null; } }
		public ModuleType Type { get; set; }
		public ExecutionType Execution { get; set; }

		public string RelativeLocation { get; set; }
		public string FileLocation
		{
			get
			{
				if (Environment != null)
				{
					return PathHelper.AddOmittedExtensionToFile(Environment.GetModulePath(Parent, RelativeLocation, Type), Type);
				}
				return null;
			}
		}

		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(RelativeLocation);
			}
		}

		public GenericModule(ILibrary parent, ModuleType type, string fileLocation)
		{
			Parent = parent;
			Type = type;
			RelativeLocation = fileLocation;
			Execution = ExecutionType.All;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1} ({2}) - {3}", Parent.Name, Path.GetFileName(RelativeLocation), RelativeLocation, EnumHelpers.ModuleTypeToString(Type));
		}
	}
}
