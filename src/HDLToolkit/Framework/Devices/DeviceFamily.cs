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
	public class DeviceFamily : IXmlSerializable
	{
		public DeviceManufacture Manufacture { get; private set; }
		public string ShortName { get; private set; } // eg acr2
		public string Name { get; private set; } // eg Automotive CoolRunner2

		// Collections of parts, packages and speeds for this family
		public List<Device> Devices { get; private set; }
		public List<DevicePackage> Packages { get; private set; }
		public List<DeviceSpeed> Speeds { get; private set; }

		public DeviceFamily(DeviceManufacture manufacture)
			: this(manufacture, null, null)
		{
		}

		public DeviceFamily(DeviceManufacture manufacture, string name, string shortName)
		{
			Manufacture = manufacture;
			Name = name;
			ShortName = shortName;
			Devices = new List<Device>();
			Packages = new List<DevicePackage>();
			Speeds = new List<DeviceSpeed>();
		}

		// Create a device/package/speed if it does not exist
		public Device CreateDevice(string name)
		{
			Device device = FindDevice(name);
			if (device == null)
			{
				device = new Device(this, name);
				Devices.Add(device);
			}
			return device;
		}

		public DevicePackage CreatePackage(string name)
		{
			DevicePackage package = FindPackage(name);
			if (package == null)
			{
				package = new DevicePackage(this, name);
				Packages.Add(package);
			}
			return package;
		}

		public DeviceSpeed CreateSpeed(string name)
		{
			DeviceSpeed speed = FindSpeed(name);
			if (speed == null)
			{
				speed = new DeviceSpeed(this, name);
				Speeds.Add(speed);
			}
			return speed;
		}

		// Find a device/package/speed
		public Device FindDevice(string name)
		{
			foreach (Device device in Devices)
			{
				if (device.Name.CompareTo(name) == 0)
				{
					return device;
				}
			}
			return null;
		}

		public DevicePackage FindPackage(string name)
		{
			foreach (DevicePackage package in Packages)
			{
				if (package.Name.CompareTo(name) == 0)
				{
					return package;
				}
			}
			return null;
		}

		public DeviceSpeed FindSpeed(string name)
		{
			foreach (DeviceSpeed speed in Speeds)
			{
				if (speed.Name.CompareTo(name) == 0)
				{
					return speed;
				}
			}
			return null;
		}

		public virtual XElement Serialize()
		{
			XElement element = new XElement("devicefamily");
			element.Add(new XAttribute("name", Name));
			element.Add(new XAttribute("shortname", ShortName));

			XElement packages = new XElement("packages");
			element.Add(packages);
			foreach (DevicePackage package in Packages)
			{
				packages.Add(package.Serialize());
			}

			XElement speeds = new XElement("speeds");
			element.Add(speeds);
			foreach (DeviceSpeed speed in Speeds)
			{
				speeds.Add(speed.Serialize());
			}

			XElement devices = new XElement("devices");
			element.Add(devices);
			foreach (Device device in Devices)
			{
				devices.Add(device.Serialize());
			}
			return element;
		}

		public virtual void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "devicefamily") == 0)
			{
				XAttribute nameAttr = element.Attribute("name");
				if (nameAttr != null)
				{
					Name = nameAttr.Value;
				}
				nameAttr = element.Attribute("shortname");
				if (nameAttr != null)
				{
					ShortName = nameAttr.Value;
				}

				XElement packages = element.Element("packages");
				foreach (XElement packageElement in packages.Elements())
				{
					DevicePackage package = new DevicePackage(this);
					package.Deserialize(packageElement);
					Packages.Add(package);
				}

				XElement speeds = element.Element("speeds");
				foreach (XElement speedElement in speeds.Elements())
				{
					DeviceSpeed speed = new DeviceSpeed(this);
					speed.Deserialize(speedElement);
					Speeds.Add(speed);
				}

				XElement devices = element.Element("devices");
				foreach (XElement deviceElement in devices.Elements())
				{
					Device device = new Device(this);
					device.Deserialize(deviceElement);
					Devices.Add(device);
				}
			}
		}
	}
}
