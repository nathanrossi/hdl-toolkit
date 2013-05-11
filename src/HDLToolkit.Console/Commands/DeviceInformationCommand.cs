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
using NConsole;
using HDLToolkit.Xilinx;
using HDLToolkit.Framework.Devices;
using HDLToolkit.Framework;

namespace HDLToolkit.Console.Commands
{
	[Command("deviceinfo")]
	public class DeviceInformationCommand : BaseCommand
	{
		[Argument(Position = 0)]
		public string Query { get; set; }

		public override void Execute()
		{
			base.Execute();

			DeviceManager manager = new DeviceManager();
			manager.Load();
			XilinxHelper.GetCurrentXilinxToolchain().LoadDevices(manager);

			// Search the entire tree to get information on the device/part/package/family/etc.
			// currently supporting search by device name, and full device name
			// e.g. xc3s100e
			// e.g. xc3s100evq100
			// e.g. xc3s100evq100-5 or xc3s100e-5vq100

			IEnumerable<object> parts = manager.FindPart(Query);
			Logger.Instance.WriteVerbose("Found {0} matching object(s)", parts.Count());

			foreach (object o in parts)
			{
				if (o is Device)
				{
					DisplayDevice(o as Device, true, true);
				}
				else if (o is DevicePart)
				{
					DisplayDevicePart(o as DevicePart, true, true);
				}
				else if (o is DevicePartSpeed)
				{
					DisplayDevicePartSpeed(o as DevicePartSpeed, true, true);
				}
			}

			//DisplayManufacture(manager.Manufacturers.FirstOrDefault(), true, true);
		}

		public static void DisplayManufacture(DeviceManufacture manufacture, bool forward, bool backward)
		{
			Logger.Instance.WriteInfo("o {0}", manufacture.Name);
			if (forward)
			{
				foreach (DeviceFamily family in manufacture.Families)
				{
					DisplayFamily(family, true, false);
				}
			}
		}

		public static void DisplayFamily(DeviceFamily family, bool forward, bool backward)
		{
			if (backward)
			{
				DisplayManufacture(family.Manufacture, false, true);
			}
			Logger.Instance.WriteInfo("  + {0} ({1}) [{2}]", family.Name, family.ShortName, family.Type);
			if (forward)
			{
				foreach (Device device in family.Devices)
				{
					DisplayDevice(device, true, false);
				}
			}
		}

		public static void DisplayDevice(Device device, bool forward, bool backward)
		{
			if (backward)
			{
				DisplayFamily(device.Family, false, true);
			}
			Logger.Instance.WriteInfo("    + {0}", device.Name);
			if (forward)
			{
				foreach (DevicePart part in device.Parts)
				{
					DisplayDevicePart(part, true, false);
				}
			}
		}

		public static void DisplayDevicePart(DevicePart part, bool forward, bool backward)
		{
			if (backward)
			{
				DisplayDevice(part.Parent, false, true);
			}
			Logger.Instance.WriteInfo("      + {0} ({1})", part.Name, part.Package.Name);
			if (forward)
			{
				foreach (DevicePartSpeed partSpeed in part.Speeds)
				{
					DisplayDevicePartSpeed(partSpeed, true, false);
				}
			}
		}

		public static void DisplayDevicePartSpeed(DevicePartSpeed partSpeed, bool forward, bool backward)
		{
			if (backward)
			{
				DisplayDevicePart(partSpeed.Part, false, true);
			}
			Logger.Instance.WriteInfo("        + {0} ({1})", partSpeed.Name, partSpeed.Speed.Name);
			foreach (ToolchainReference reference in partSpeed.Toolchains)
			{
				Logger.Instance.WriteInfo("          + Supported by: '{0}'", reference.Id);
			}
		}
	}
}
