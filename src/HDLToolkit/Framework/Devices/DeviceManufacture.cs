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
	public abstract class DeviceManufacture
	{
		public virtual string Name { get { return null; } }

		// The families of devices for this manufacture
		public List<DeviceFamily> Families { get; private set; }

		protected DeviceManufacture()
		{
			Families = new List<DeviceFamily>();
		}

		// Find a family
		public DeviceFamily FindFamily(string name)
		{
			foreach (DeviceFamily family in Families)
			{
				if (family.ShortName.CompareTo(name) == 0)
				{
					return family;
				}
			}
			return null;
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("devicemanufacture");
			element.Add(new XAttribute("name", Name));

			XElement families = new XElement("families");
			element.Add(families);
			foreach (DeviceFamily family in Families)
			{
				families.Add(family.Serialize());
			}

			return element;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "devicemanufacture") == 0)
			{
				XElement families = element.Element("families");
				foreach (XElement familyElement in families.Elements())
				{
					DeviceFamily family = new DeviceFamily(this);
					family.Deserialize(familyElement);
					Families.Add(family);
				}
			}
		}
	}
}
