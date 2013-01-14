using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NConsole;
using System.IO;
using HDLToolkit.Xilinx.Implementation.BlockRAM;

namespace HDLToolkit.Console.Commands.Xilinx
{
	[Command("xilinxbitinject")]
	public class BitInjectorCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string Output { get; set; }

		[Argument(ShortName = "m", LongName = "bmm")]
		public string BlockMemoryMap { get; set; }

		[Argument(ShortName = "b", LongName = "bitstream")]
		public string Bitstream { get; set; }

		[Argument(Position = 0)]
		public string Binary { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (string.IsNullOrEmpty(Output) || !Directory.Exists(Output))
			{
				Logger.Instance.WriteError("Output Path '{0}' does not exist", Output);
				return;
			}

			string bmmfile = PathHelper.GetFullPath(BlockMemoryMap);
			string bitfile = PathHelper.GetFullPath(Bitstream);
			string binary = PathHelper.GetFullPath(Binary);
			Logger.Instance.WriteVerbose("Selected BMM '{0}'", bmmfile);
			Logger.Instance.WriteVerbose("Selected Bitstream '{0}'", bitfile);
			Logger.Instance.WriteVerbose("Selected Binary '{0}'", binary);

			OutputPath location = new OutputPath();
			location.OutputDirectory = PathHelper.GetFullPath(Output);
			location.TemporaryDirectory = SystemHelper.GetTemporaryDirectory();
			location.WorkingDirectory = Environment.CurrentDirectory;
			location.LogDirectory = location.OutputDirectory;

			Logger.Instance.WriteVerbose("Starting Injection");
			BitstreamDataInjector bitInjector = new BitstreamDataInjector(location);
			bitInjector.BMMDescription = bmmfile;
			bitInjector.Bitstream = bitfile;
			bitInjector.BinaryFile = binary;
			if (bitInjector.Build())
			{
				Logger.Instance.WriteInfo("Injection Complete");
			}
			else
			{
				Logger.Instance.WriteError("Injection Failed");
			}

			Logger.Instance.WriteVerbose("Cleaning temporary directory");
			Directory.Delete(location.TemporaryDirectory, true);
		}
	}
}
