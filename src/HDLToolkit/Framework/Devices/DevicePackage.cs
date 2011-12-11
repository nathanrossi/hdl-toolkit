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
using System.Xml.Linq;

namespace HDLToolkit.Framework.Devices
{
	public class DevicePackage : IXmlSerializable
	{
		public string Name { get; private set; }
		public DeviceFamily Family { get; private set; }

		public DevicePackage(DeviceFamily family)
			: this(family, null)
		{
		}

		public DevicePackage(DeviceFamily family, string name)
		{
			Family = family;
			Name = name;
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("devicepackage");
			element.Add(new XAttribute("name", Name));
			return element;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "devicepackage") == 0)
			{
				XAttribute nameAttr = element.Attribute("name");
				if (nameAttr != null)
				{
					Name = nameAttr.Value;
				}
			}
		}
	}
}
