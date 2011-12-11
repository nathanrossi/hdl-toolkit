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
	public class DeviceManager
	{
		public List<DeviceManufacture> Manufacturers { get; private set; }

		public DeviceManager()
		{
			Manufacturers = new List<DeviceManufacture>();
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
	}
}
