using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HDLToolkit.Framework.Devices;
using HDLToolkit.Xilinx.Parsers;

namespace HDLToolkit.Xilinx.Implementation
{
	public class NGDBuilder
	{
		public XilinxToolchain Toolchain { get; private set; }
		public OutputPath OutputLocation { get; private set; }

		public string NetList { get; set; }
		public string ConstraintsFile { get; set; }
		public DevicePartSpeed TargetDevice { get; set; }

		public NGDBuilder(XilinxToolchain toolchain, OutputPath output)
		{
			Toolchain = toolchain;
			OutputLocation = output;
		}

		public bool Build()
		{
			string projectName = Path.GetFileNameWithoutExtension(NetList);

			string projectNgoPath = PathHelper.Combine(OutputLocation.TemporaryDirectory, "ngo");
			string projectNgdFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.ngd", projectName));
			string projectBldFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.bld", projectName));
			
			// Target Device
			string targetDeviceName = TargetDevice.AlternateName;
			Logger.Instance.WriteDebug("Target Device Name: {0}", targetDeviceName);
			
			// Setup Arguments
			List<string> arguments = new List<string>();

			// Specify the output path
			arguments.Add(string.Format("-dd \"{0}\"", projectNgoPath));

			// Specify the constraints file
			if (string.IsNullOrEmpty(ConstraintsFile))
			{
				// Ignore the constraints file
				arguments.Add("-i");
			}
			else if (File.Exists(ConstraintsFile))
			{
				arguments.Add(string.Format("-uc \"{0}\"", ConstraintsFile));
			}
			else
			{
				throw new FileNotFoundException("Constraints File does not exist.");
			}

			// Ignore timestamps, always run
			arguments.Add("-nt on");

			// Target Device
			arguments.Add(string.Format("-p {0}", targetDeviceName));

			arguments.Add("-verbose");

			// The source netlist
			if (string.IsNullOrEmpty(NetList) || !File.Exists(NetList))
			{
				throw new FileNotFoundException("NetList File does not exist.");
			}
			arguments.Add(string.Format("\"{0}\"", NetList));

			// The output NGD
			arguments.Add(string.Format("\"{0}\"", projectNgdFilePath));

			// Create Temporary Directories
			Directory.CreateDirectory(projectNgoPath);
			Logger.Instance.WriteDebug("Created Temporary Directory (ngo): {0}", projectNgoPath);

			// Prepare Process
			XilinxProcess process = new XilinxProcess(Toolchain, "ngdbuild", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			// Copy logs to the log directory
			OutputLocation.CopyLogFile(projectBldFilePath);

			// Copy Artifacts to output directory
			OutputLocation.CopyOutputFile(projectNgdFilePath);

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0 || !File.Exists(projectNgdFilePath))
			{
				return false;
			}

			return true;
		}
	}
}
