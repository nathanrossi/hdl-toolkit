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
using HDLToolkit.Framework;
using HDLToolkit.Xilinx.Synthesis;
using System.IO;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Console.Commands
{
	[Command("coresyn")]
	public class CoreSynthesizeCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string Output { get; set; }

		[Argument(ShortName = "d", LongName = "device")]
		public string Device { get; set; }

		[Argument(Position = 0)]
		public string Module { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (string.IsNullOrEmpty(Output) || !Directory.Exists(Output))
			{
				Logger.Instance.WriteError("Output Path '{0}' does not exist", Output);
				return;
			}

			IModule module = Program.Repository.FindModuleByName(Module);
			if (module == null)
			{
				Logger.Instance.WriteError("Cannot Find Module '{0}'", Module);
				return;
			}
			Logger.Instance.WriteVerbose("Selected module '{0}' in library '{1}'", module.Name, module.Parent.Name);

			// Load Device Manager
			DeviceManager manager = new DeviceManager();
			XilinxDeviceTree deviceTree = new XilinxDeviceTree();
			deviceTree.Load();
			manager.Manufacturers.Add(deviceTree);
			
			// Search for Part
			DevicePartSpeed device = null;
			IEnumerable<object> parts = manager.FindPart(Device);
			object firstPart = parts.First();
			Logger.Instance.WriteVerbose("Found {0} matching device(s)", parts.Count());
			
			if (firstPart == null || !(firstPart is DevicePartSpeed))
			{
				Logger.Instance.WriteError("Cannot Find Device '{0}'", Device);
				return;
			}
			device = firstPart as DevicePartSpeed;
			Logger.Instance.WriteVerbose("Selected device '{0}'", device.Name);

			OutputPath location = new OutputPath();
			location.OutputDirectory = PathHelper.GetFullPath(Output);
			location.TemporaryDirectory = SystemHelper.GetTemporaryDirectory();
			location.WorkingDirectory = Environment.CurrentDirectory;
			location.LogDirectory = location.OutputDirectory;
			
			Logger.Instance.WriteVerbose("Starting Build");
			bool successful = false;
			using (XilinxSynthesizer synthesizer = new XilinxSynthesizer(location, module, device))
			{
				successful = synthesizer.Build();
			}

			if (successful)
			{
				Logger.Instance.WriteInfo("Build Complete");
			}
			else
			{
				Logger.Instance.WriteError("Build Failed");
			}

			Logger.Instance.WriteVerbose("Cleaning temporary directory");
			Directory.Delete(location.TemporaryDirectory, true);
		}
	}
}
