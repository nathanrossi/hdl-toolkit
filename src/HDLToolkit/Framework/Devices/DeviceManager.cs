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
using System.Xml.Linq;

namespace HDLToolkit.Framework.Devices
{
	public class DeviceManager : IXmlSerializable
	{
		public List<DeviceManufacture> Manufacturers { get; private set; }
		public HashSet<ToolchainReference> CachedToolchains { get; private set; }

		public bool AllowCaching { get; set; }

		public DeviceManager()
		{
			AllowCaching = true;
			Manufacturers = new List<DeviceManufacture>();
			CachedToolchains = new HashSet<ToolchainReference>();
		}

		public DeviceManufacture CreateManufacture(string name)
		{
			DeviceManufacture manufacture = FindManufacture(name);
			if (manufacture == null)
			{
				manufacture = new DeviceManufacture(this, name);
				Manufacturers.Add(manufacture);
			}
			return manufacture;
		}

		public DeviceManufacture FindManufacture(string name)
		{
			foreach (DeviceManufacture manufacture in Manufacturers)
			{
				if (string.Compare(manufacture.Name, name, true) == 0)
				{
					return manufacture;
				}
			}
			return null;
		}

		public IEnumerable<object> FindPart(string query)
		{
			List<object> devices = new List<object>();
			foreach (DeviceManufacture manufacture in Manufacturers)
			{
				foreach (DeviceFamily family in manufacture.Families)
				{
					foreach (Device device in family.Devices)
					{
						// does if match the device?
						if (string.Compare(device.Name, query, true) == 0)
						{
							devices.Add(device);
						}

						foreach (DevicePart part in device.Parts)
						{
							// does if match the part?
							if (string.Compare(part.Name, query, true) == 0)
							{
								devices.Add(part);
							}

							foreach (DevicePartSpeed speed in part.Speeds)
							{
								// does if match the part and speed?
								if (string.Compare(speed.Name, query, true) == 0)
								{
									devices.Add(speed);
								}
								else if (string.Compare(speed.AlternateName, query, true) == 0)
								{
									devices.Add(speed);
								}
							}
						}
					}
				}
			}
			return devices;
		}

		private static string GetCacheFile()
		{
			string path = SystemHelper.GetCacheDirectory();
			path = PathHelper.Combine(path, "devices");
			Directory.CreateDirectory(path);
			path = PathHelper.Combine(path, string.Format("cache.xml"));
			Logger.Instance.WriteDebug("Device cache file located at '{0}'", path);
			return path;
		}

		public void Load()
		{
			if (AllowCaching && File.Exists(GetCacheFile()))
			{
				LoadFromCache();
			}
		}

		public void Save()
		{
			if (AllowCaching)
			{
				SaveToCache();
			}
		}

		private void LoadFromCache()
		{
			Logger.Instance.WriteVerbose("Loading Device Library from cache");
			string path = GetCacheFile();
			XDocument document;
			using (FileStream stream = new FileStream(path, FileMode.Open))
			{
				using (StreamReader reader = new StreamReader(stream))
				{
					document = XDocument.Load(reader);
				}
			}
			Deserialize(document.Elements().First());
		}

		private void SaveToCache()
		{
			Logger.Instance.WriteVerbose("Saving Device Library to cache");
			string path = GetCacheFile();
			XDocument document = new XDocument(Serialize());
			using (FileStream stream = new FileStream(path, FileMode.CreateNew))
			{
				using (StreamWriter writer = new StreamWriter(stream))
				{
					document.Save(writer);
				}
			}
		}

		public XElement Serialize()
		{
			XElement element = new XElement("devicemanager");
			
			// Stored cached info
			XElement cached = new XElement("cached");
			element.Add(cached);
			foreach (ToolchainReference reference in CachedToolchains)
			{
				cached.Add(reference.Serialize());
			}

			// Store Manufacture info
			XElement manufacturers = new XElement("manufacturers");
			element.Add(manufacturers);
			foreach (DeviceManufacture manufacture in Manufacturers)
			{
				manufacturers.Add(manufacture.Serialize());
			}

			return element;
		}

		public void Deserialize(XElement element)
		{
			if (string.Compare(element.Name.ToString(), "devicemanager") == 0)
			{
				// Parse the speeds
				XElement cached = element.Element("cached");
				if (cached != null)
				{
					foreach (XElement cachedElement in cached.Elements())
					{
						ToolchainReference reference = new ToolchainReference();
						reference.Deserialize(cachedElement);
						CachedToolchains.Add(reference);
					}
				}

				// Parse the manufacturers
				XElement manufacturers = element.Element("manufacturers");
				if (manufacturers != null)
				{
					foreach (XElement manufacturerElement in manufacturers.Elements())
					{
						DeviceManufacture manufacturer = new DeviceManufacture(this);
						manufacturer.Deserialize(manufacturerElement);
						Manufacturers.Add(manufacturer);
					}
				}
			}
		}
	}
}
