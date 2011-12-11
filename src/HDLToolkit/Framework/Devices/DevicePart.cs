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
	public class DevicePart : IXmlSerializable
	{
		public string Name
		{
			get
			{
				if (Parent != null && Package != null)
				{
					return Parent.Name + Package.Name;
				}
				return null;
			}
		}
		public Device Parent { get; private set; }

		public DevicePackage Package { get; private set; }
		public List<DevicePartSpeed> Speeds { get; private set; }

		public DevicePart(Device device)
			: this (device, null)
		{
		}

		public DevicePart(Device device, DevicePackage package)
		{
			Parent = device;
			Package = package;
			Speeds = new List<DevicePartSpeed>();
		}

		public DevicePartSpeed CreateSpeed(DeviceSpeed speed)
		{
			DevicePartSpeed partSpeed = FindSpeed(speed);
			if (partSpeed == null)
			{
				partSpeed = new DevicePartSpeed(this, speed);
				Speeds.Add(partSpeed);
			}
			return partSpeed;
		}

		public DevicePartSpeed FindSpeed(DeviceSpeed speed)
		{
			foreach (DevicePartSpeed partSpeed in Speeds)
			{
				if (partSpeed.Speed.Equals(speed))
				{
					return partSpeed;
				}
			}
			return null;
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("devicepart");
			element.Add(new XAttribute("package", Package.Name));
			XElement speeds = new XElement("speeds");
			element.Add(speeds);
			foreach (DevicePartSpeed speed in Speeds)
			{
				speeds.Add(speed.Serialize());
			}
			return element;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "devicepart") == 0)
			{
				// Parse the package
				XAttribute packageAttr = element.Attribute("package");
				if (packageAttr != null)
				{
					Package = Parent.Family.FindPackage(packageAttr.Value);
				}
				// Parse the speeds
				XElement speeds = element.Element("speeds");
				if (speeds != null)
				{
					foreach (XElement speedElement in speeds.Elements())
					{
						DevicePartSpeed speed = new DevicePartSpeed(this);
						speed.Deserialize(speedElement);
						Speeds.Add(speed);
					}
				}
			}
		}
	}
}
