﻿// Copyright 2011 Nathan Rossi - http://nathanrossi.com
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
using HDLToolkit.Console.Helpers;
using HDLToolkit.Framework.Synthesis;

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

			GenericSynthesisConfiguration configuration = new GenericSynthesisConfiguration();
			configuration.Module = Program.Repository.FindModuleByName(Module);
			configuration.TargetDevice = DeviceHelper.FindDeviceByName(Device);

			if (configuration.Module == null)
			{
				Logger.Instance.WriteError("Cannot Find Module '{0}'", Module);
				return;
			}
			Logger.Instance.WriteVerbose("Selected module '{0}' in library '{1}'", configuration.Module.Name, configuration.Module.Parent.Name);

			if (configuration.TargetDevice == null)
			{
				Logger.Instance.WriteError("Cannot Find Device '{0}'", Device);
				return;
			}
			Logger.Instance.WriteVerbose("Selected device '{0}'", configuration.TargetDevice.Name);

			OutputPath location = new OutputPath();
			location.OutputDirectory = PathHelper.GetFullPath(Output);
			location.TemporaryDirectory = SystemHelper.GetTemporaryDirectory();
			location.WorkingDirectory = Environment.CurrentDirectory;
			location.LogDirectory = location.OutputDirectory;

			Logger.Instance.WriteVerbose("Starting Build");
			bool successful = false;
			ISynthesizer synthesizer = XilinxHelper.GetCurrentXilinxToolchain().Synthesizers.FirstOrDefault();
			if (synthesizer != null)
			{
				Logger.Instance.WriteDebug("Using '{0}' Synthesizer from '{1}' toolchain", synthesizer.GetType().Name, synthesizer.Toolchain.GetType().Name);
				using (ISynthesizerInstance instance = synthesizer.Create(location, configuration))
				{
					successful = instance.Build();
				}
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
