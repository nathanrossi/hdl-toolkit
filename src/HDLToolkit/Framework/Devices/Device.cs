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
	public class Device : IXmlSerializable
	{
		public string Name { get; private set; }
		public DeviceFamily Family { get; private set; }

		public List<DevicePart> Parts { get; private set; }

		public Device(DeviceFamily family)
			: this(family, null)
		{
		}

		public Device(DeviceFamily family, string name)
		{
			Family = family;
			Name = name;
			Parts = new List<DevicePart>();
		}

		public DevicePart CreatePart(DevicePackage package)
		{
			DevicePart part = FindPart(package);
			if (part == null)
			{
				part = new DevicePart(this, package);
				Parts.Add(part);
			}
			return part;
		}

		public DevicePart FindPart(DevicePackage package)
		{
			foreach (DevicePart part in Parts)
			{
				if (part.Package.Equals(package))
				{
					return part;
				}
			}
			return null;
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("device");
			element.Add(new XAttribute("name", Name));
			XElement parts = new XElement("parts");
			element.Add(parts);
			foreach (DevicePart part in Parts)
			{
				parts.Add(part.Serialize());
			}
			return element;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "device") == 0)
			{
				XAttribute nameAttr = element.Attribute("name");
				if (nameAttr != null)
				{
					Name = nameAttr.Value;
				}
				XElement parts = element.Element("parts");
				foreach (XElement partElement in parts.Elements())
				{
					DevicePart part = new DevicePart(this);
					part.Deserialize(partElement);
					Parts.Add(part);
				}
			}
		}
	}
}
