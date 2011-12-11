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
using System.IO;
using HDLToolkit.Framework.Devices;
using HDLToolkit.Console.Helpers;

namespace HDLToolkit.Console.Commands
{
	[Command("corexisegen")]
	public class CoreXiseGenCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string OutputPath { get; set; }

		[Argument(ShortName = "u", LongName = "ucf")]
		public string UserConstraintsFile { get; set; }

		[Argument(ShortName = "d", LongName = "device")]
		public string DeviceQueryString { get; set; }

		[Argument(Position = 0)]
		public string[] Cores { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (!string.IsNullOrEmpty(OutputPath))
			{
				XilinxProjectFile prj = new XilinxProjectFile(Program.Repository);

				if (!string.IsNullOrEmpty(DeviceQueryString))
				{
					DeviceManager manager = new DeviceManager();
					XilinxDeviceTree devTree = new XilinxDeviceTree();
					devTree.Load();
					manager.Manufacturers.Add(devTree);

					prj.Device = GetPart(manager, DeviceQueryString);
					if (prj.Device == null)
					{
						Logger.Instance.WriteError("Could not found a device to match the string '{0}'", DeviceQueryString);
						return;
					}
					else
					{
						Logger.Instance.WriteVerbose("Selected device '{0}'", prj.Device.Name);
					}
				}
				
				if (!string.IsNullOrEmpty(UserConstraintsFile))
				{
					prj.UserConstraintsFile = Path.GetFullPath(UserConstraintsFile);
					Logger.Instance.WriteVerbose("Including the user constaints located at '{0}'", prj.UserConstraintsFile);
				}

				foreach (string core in Cores)
				{
					Logger.Instance.WriteVerbose("Selecting Core: {0}", core);
					prj.AddAllInLibrary(prj.Environment.GetLibrary(core));
				}

				Logger.Instance.WriteVerbose("Generating...");
				File.WriteAllText(OutputPath, prj.ToString());
				Logger.Instance.WriteVerbose("Generated!");
			}
			else
			{
				Logger.Instance.WriteError("Output Path not specified, terminating...");
				return;
			}
		}

		private static DevicePartSpeed GetPart(DeviceManager manager, string query)
		{
			object o = DeviceHelper.FindDevice(manager, query);
			
			Device device = o as Device;
			if (device != null)
			{
				if (device.Parts.Count != 0)
				{
					Logger.Instance.WriteWarning("Found device, unable to match with package specifier, selecting default package '{0}'", device.Parts[0].Package.Name);
					o = device.Parts[0];
				}
			}

			DevicePart part = o as DevicePart;
			if (part != null)
			{
				if (part.Speeds.Count != 0)
				{
					Logger.Instance.WriteWarning("Found device, unable to match with speed specifier, selecting default speed '{0}'", part.Speeds[0].Speed.Name);
					o = part.Speeds[0];
				}
			}

			return (o as DevicePartSpeed);
		}
	}
}
