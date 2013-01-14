using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HDLToolkit.Xilinx.Parsers;

namespace HDLToolkit.Xilinx.Implementation.BlockRAM
{
	public class BitstreamDataInjector
	{
		public OutputPath OutputLocation { get; private set; }

		public string Bitstream { get; set; }
		public string BMMDescription { get; set; }
		public string BinaryFile { get; set; }

		public BitstreamDataInjector(OutputPath output)
		{
			OutputLocation = output;
		}

		public bool Build()
		{
			string projectName = Path.GetFileNameWithoutExtension(Bitstream);

			string projectMemFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.mem", projectName));
			string projectBitFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}_mem.bit", projectName));

			// Check bitstream file exists
			if (string.IsNullOrEmpty(Bitstream) || !File.Exists(Bitstream))
			{
				throw new FileNotFoundException("Bitstream File does not exist.");
			}
			// Check bmm file exists
			if (string.IsNullOrEmpty(BMMDescription) || !File.Exists(BMMDescription))
			{
				throw new FileNotFoundException("BMM File does not exist.");
			}
			// Check binary file exists
			if (string.IsNullOrEmpty(BinaryFile) || !File.Exists(BinaryFile))
			{
				throw new FileNotFoundException("Binary File does not exist.");
			}

			// Generate the mem file from binary data
			string data = MemFormatHelper.ConvertBinaryToMem(File.ReadAllBytes(BinaryFile));
			File.WriteAllText(projectMemFilePath, data);

			// Setup Arguments
			List<string> arguments = new List<string>();

			// The BMM
			arguments.Add(string.Format("-bm \"{0}\"", BMMDescription));

			// The memory contents
			arguments.Add(string.Format("-bd \"{0}\"", projectMemFilePath));

			// The source bitstream
			arguments.Add(string.Format("-bt \"{0}\"", Bitstream));

			// The output bitstream
			arguments.Add(string.Format("-o b \"{0}\"", projectBitFilePath));

			// Prepare Process
			XilinxProcess process = new XilinxProcess("data2mem", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			StringProcessListener stdout = new StringProcessListener();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.Listeners.Add(stdout);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			Logger.Instance.WriteDebug(stdout.Output);

			// Copy results to output
			OutputLocation.CopyOutputFile(projectBitFilePath);

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0 || !File.Exists(projectBitFilePath))
			{
				return false;
			}
			return true;
		}
	}
}
