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

namespace HDLToolkit.Framework.Devices
{
	public interface IPartFamily
	{
		string ShortName { get; } // eg acr2
		string Name { get; } // eg Automotive CoolRunner2

		// The parts for this family
		IList<IPart> Parts { get; }

		// The valid Packages for the family
		IEnumerable<IPartPackage> Packages { get; }

		// The valid Speeds for the parts in the family
		IEnumerable<IPartSpeed> Speeds { get; }

		// Create a package/speed if it does not exist
		IPartPackage CreatePackage(string name);
		IPartSpeed CreateSpeed(string name);

		// Find a package/speed
		IPartPackage FindPackage(string name);
		IPartSpeed FindSpeed(string name);
	}
}
