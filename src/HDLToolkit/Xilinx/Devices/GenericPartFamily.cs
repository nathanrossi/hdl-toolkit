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
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Devices
{
	public class GenericPartFamily : IPartFamily
	{
		public string ShortName { get; set; }
		public string Name { get; set; }

		public IList<IPart> Parts { get; private set; }

		List<IPartSpeed> speeds;
		List<IPartPackage> packages;
		public IEnumerable<IPartSpeed> Speeds { get { return speeds; } }
		public IEnumerable<IPartPackage> Packages { get { return packages; } }

		public GenericPartFamily(string name, string shortname)
		{
			Name = name;
			ShortName = shortname;

			Parts = new List<IPart>();
			speeds = new List<IPartSpeed>();
			packages = new List<IPartPackage>();
		}

		public GenericPart CreatePart(string name)
		{
			GenericPart part = new GenericPart(this, name);
			this.Parts.Add(part);
			return part;
		}

		public IPartPackage CreatePackage(string name)
		{
			IPartPackage create = FindPackage(name);
			if (create == null)
			{
				create = new GenericPartPackage(this, name);
				packages.Add(create);
			}
			return create;
		}

		public IPartSpeed CreateSpeed(string name)
		{
			IPartSpeed create = FindSpeed(name);
			if (create == null)
			{
				create = new GenericPartSpeed(this, name);
				speeds.Add(create);
			}
			return create;
		}

		public IPartPackage FindPackage(string name)
		{
			foreach (IPartPackage package in packages)
			{
				if (package.Name.CompareTo(name) == 0)
				{
					return package;
				}
			}
			return null;
		}

		public IPartSpeed FindSpeed(string name)
		{
			foreach (IPartSpeed speed in speeds)
			{
				if (speed.Name.CompareTo(name) == 0)
				{
					return speed;
				}
			}
			return null;
		}
	}
}
