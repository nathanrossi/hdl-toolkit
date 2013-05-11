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
using HDLToolkit.Xilinx;

namespace HDLToolkit.Console.Helpers
{
	public static class DeviceHelper
	{
		public static object FindDevice(DeviceManager manager, string query)
		{
			IEnumerable<object> parts = manager.FindPart(query);
			Logger.Instance.WriteDebug("Found {0} matching parts", parts.Count());

			Device topDevice = null;
			DevicePart topDevicePart = null;
			DevicePartSpeed topDevicePartSpeed = null;
			foreach (object o in parts)
			{
				if (o is Device)
				{
					topDevice = o as Device;
				}
				else if (o is DevicePart)
				{
					topDevicePart = o as DevicePart;
				}
				else if (o is DevicePartSpeed)
				{
					topDevicePartSpeed = o as DevicePartSpeed;
					// pick the first one of these
					break;
				}
			}

			if (topDevicePartSpeed != null)
			{
				return topDevicePartSpeed;
			}
			if (topDevicePart != null)
			{
				return topDevicePart;
			}
			if (topDevice != null)
			{
				return topDevice;
			}
			return null;
		}

		public static DevicePartSpeed FindDeviceByName(string query)
		{
			// Load Device Manager
			DeviceManager manager = new DeviceManager();
			manager.Load();
			return FindDeviceByName(manager, query);
		}

		public static DevicePartSpeed FindDeviceByName(DeviceManager manager, string query)
		{
			DevicePartSpeed device = null;
			IEnumerable<object> parts = manager.FindPart(query);
			object firstPart = parts.FirstOrDefault();
			Logger.Instance.WriteVerbose("Found {0} matching device(s)", parts.Count());

			if (firstPart == null || !(firstPart is DevicePartSpeed))
			{
				return null;
			}
			device = firstPart as DevicePartSpeed;

			return device;
		}
	}
}
