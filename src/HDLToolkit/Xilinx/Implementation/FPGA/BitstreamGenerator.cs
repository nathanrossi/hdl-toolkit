using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HDLToolkit.Xilinx.Parsers;

namespace HDLToolkit.Xilinx.Implementation.FPGA
{
	public class BitstreamGenerator
	{
		public XilinxToolchain Toolchain { get; private set; }

		public string NCDFile { get; private set; }

		public OutputPath OutputLocation  { get; private set; }

		public BitstreamGenerator(XilinxToolchain toolchain, OutputPath output, string ncd)
		{
			Toolchain = toolchain;
			OutputLocation = output;
			NCDFile = ncd;
		}

		public bool Build()
		{
			string bitFile = PathHelper.Combine(OutputLocation.TemporaryDirectory,
					string.Format("{0}.bit", Path.GetFileNameWithoutExtension(NCDFile)));

			// Setup Arguments
			List<string> arguments = new List<string>();

			// Default configuration
			arguments.Add("-w"); // Overwrite existing files

			// The Input NCD
			if (string.IsNullOrEmpty(NCDFile) || !File.Exists(NCDFile))
			{
				throw new FileNotFoundException("NCD File does not exist.");
			}
			arguments.Add(string.Format("\"{0}\"", NCDFile));

			// Prepare Process
			XilinxProcess process = new XilinxProcess(Toolchain, "bitgen", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			// Copy Artifacts to output directory
			OutputLocation.CopyOutputFile(bitFile);

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0 || !File.Exists(bitFile))
			{
				return false;
			}

			return true;
		}
	}
}
