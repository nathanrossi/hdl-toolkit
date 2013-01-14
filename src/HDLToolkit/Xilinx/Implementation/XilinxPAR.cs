using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HDLToolkit.Xilinx.Parsers;

namespace HDLToolkit.Xilinx.Implementation
{
	public class XilinxPAR
	{
		public OutputPath OutputLocation { get; private set; }

		public string NCDFile { get; set; }
		public string PCFFile { get; set; }

		public XilinxPAR(OutputPath output)
		{
			OutputLocation = output;
		}

		public bool Build()
		{
			string projectName = Path.GetFileNameWithoutExtension(NCDFile);

			string projectNcdFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.ncd", projectName));
			string projectParFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.par", projectName));
			string projectGrfFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.grf", projectName));
			
			// Setup Arguments
			List<string> arguments = new List<string>();

			// Default configuration
			arguments.Add("-w"); // Overwrite existing files
			arguments.Add("-ol high"); // Effort Level
			arguments.Add("-mt off"); // Multi-Thread execution not avaliable on all parts

			// The Input NCD
			if (string.IsNullOrEmpty(NCDFile) || !File.Exists(NCDFile))
			{
				throw new FileNotFoundException("NCD File does not exist.");
			}
			arguments.Add(string.Format("\"{0}\"", NCDFile));

			// Output NCD File
			arguments.Add(string.Format("\"{0}\"", projectNcdFilePath));

			// The Input PCF
			if (string.IsNullOrEmpty(PCFFile) || !File.Exists(PCFFile))
			{
				throw new FileNotFoundException("PCF File does not exist.");
			}
			arguments.Add(string.Format("\"{0}\"", PCFFile));

			// Prepare Process
			XilinxProcess process = new XilinxProcess("par", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			// Copy logs to the log directory
			OutputLocation.CopyLogFile(projectParFilePath);
			OutputLocation.CopyLogFile(projectGrfFilePath);

			// Copy Artifacts to output directory
			OutputLocation.CopyOutputFile(projectNcdFilePath);

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0 || !File.Exists(projectNcdFilePath))
			{
				return false;
			}

			return true;
		}
	}
}
