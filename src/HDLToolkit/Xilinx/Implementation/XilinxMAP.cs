using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Devices;
using System.IO;
using HDLToolkit.Xilinx.Parsers;

namespace HDLToolkit.Xilinx.Implementation
{
	public class XilinxMAP
	{
		public OutputPath OutputLocation { get; private set; }

		public string NGDFile { get; set; }
		public DevicePartSpeed TargetDevice { get; set; }

		public XilinxMAP(OutputPath output)
		{
			OutputLocation = output;
		}

		public bool Build()
		{
			string projectName = Path.GetFileNameWithoutExtension(NGDFile);

			string projectPcfFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.pcf", projectName));
			string projectNcdFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.ncd", projectName));
			string projectMrpFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.mrp", projectName));
			
			// Target Device
			string targetDeviceName = TargetDevice.AlternateName;
			Logger.Instance.WriteDebug("Target Device Name: {0}", targetDeviceName);
			
			// Setup Arguments
			List<string> arguments = new List<string>();

			// Target Device
			arguments.Add(string.Format("-p {0}", targetDeviceName));

			// Default configuration
			arguments.Add("-w"); // Overwrite existing files
			arguments.Add("-logic_opt off"); // Post Place Logic Optimization
			arguments.Add("-ol high"); // Effort Level
			// arguments.Add("-t 1");
			// arguments.Add("-xt 0");
			// arguments.Add("-r 4");
			arguments.Add("-global_opt off");
			arguments.Add("-mt off"); // Multi-Thread execution not avaliable on all parts
			arguments.Add("-ir off");
			// arguments.Add("-pr off"); // Register Packing
			// arguments.Add("-lc off"); // LUT Combining
			arguments.Add("-power off"); // Power Optimization

			// Output NCD File
			arguments.Add(string.Format("-o \"{0}\"", projectNcdFilePath));

			// The source netlist
			if (string.IsNullOrEmpty(NGDFile) || !File.Exists(NGDFile))
			{
				throw new FileNotFoundException("NGD File does not exist.");
			}
			arguments.Add(string.Format("\"{0}\"", NGDFile));

			// The output pcf
			arguments.Add(string.Format("\"{0}\"", projectPcfFilePath));

			// Prepare Process
			XilinxProcess process = new XilinxProcess("map", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			// Copy logs to the log directory
			OutputLocation.CopyLogFile(projectMrpFilePath);

			// Copy Artifacts to output directory
			OutputLocation.CopyOutputFile(projectNcdFilePath);
			OutputLocation.CopyOutputFile(projectPcfFilePath);

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0 || !File.Exists(projectNcdFilePath))
			{
				return false;
			}

			return true;
		}
	}
}
