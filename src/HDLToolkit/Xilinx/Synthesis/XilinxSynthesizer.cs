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
using HDLToolkit.Framework;
using System.IO;
using HDLToolkit.Xilinx.Parsers;
using HDLToolkit.Framework.Synthesis;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.Xilinx.Synthesis
{
	public class XilinxSynthesizer : ISynthesizer
	{
		public OutputPath OutputLocation { get; private set; }
		public IModule Module { get; private set; }
		public DevicePartSpeed TargetDevice { get; private set; }
		public List<string> Artifacts { get; private set; }
		public Dictionary<string, string> Configuration  { get; private set; }

		public XilinxSynthesizer(OutputPath output, IModule module, DevicePartSpeed device)
		{
			OutputLocation = output;
			Module = module;
			TargetDevice = device;

			Artifacts = new List<string>();
			Configuration = new Dictionary<string, string>();
		}

		public bool Build()
		{
			string projectName = Module.Name;

			// Location of scripts and project files
			string projectFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, String.Format("{0}.prj", projectName));
			string projectXstFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, String.Format("{0}.xst", projectName));
			string projectSyrFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, String.Format("{0}.syr", projectName));
			string projectXstPath = PathHelper.Combine(OutputLocation.TemporaryDirectory, "xst");
			string projectTmpPath = PathHelper.Combine(projectXstPath, ".tmp");
			string netlistName = string.Format("{0}.ngc", projectName);

			// Generate Project File
			PrjFile project = PrjFile.CreateFromIModule(Module);
			File.WriteAllText(projectFilePath, project.ToString(ExecutionType.SynthesisOnly));

			// Synthesis Module name
			string topLevelModuleName = string.Format("{0}.{1}", Module.Parent.Name, Module.Name);
			Logger.Instance.WriteDebug("Top Level Module Name: {0}", topLevelModuleName);
			// Synthesis Target Device
			string targetDeviceName = TargetDevice.AlternateName;
			Logger.Instance.WriteDebug("Target Device Name: {0}", targetDeviceName);

			// Create Configuration
			XilinxSynthesisConfiguration config = new XilinxSynthesisConfiguration(OutputLocation);
			config.ProjectFilePath = projectFilePath;
			config.TargetDevice = targetDeviceName;
			config.TopModuleName = Module.Name;
			config.OutputFileName = netlistName;
			File.WriteAllText(projectXstFilePath, config.GenerateScript());

			// Create Temporary Directories
			Directory.CreateDirectory(projectXstPath);
			Directory.CreateDirectory(projectTmpPath);
			Logger.Instance.WriteDebug("Created Temporary Directory (xst): {0}", projectXstPath);
			Logger.Instance.WriteDebug("Created Temporary Directory (tmp): {0}", projectTmpPath);

			// Prepare Process Arguments
			List<string> arguments = new List<string>();
			arguments.Add(string.Format("-ifn \"{0}\"", projectXstFilePath));
			arguments.Add(string.Format("-ofn \"{0}\"", projectSyrFilePath));

			// Prepare Process
			XilinxProcess process = new XilinxProcess("xst", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			// Copy logs to the log directory
			OutputLocation.CopyLogFile(projectSyrFilePath);

			// Copy Artifacts to output directory
			OutputLocation.CopyOutputFile(PathHelper.Combine(OutputLocation.TemporaryDirectory, netlistName));

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0)
			{
				return false;
			}

			return true;
		}

		public void Dispose()
		{
			// Nothing to dispose of
		}
	}
}
