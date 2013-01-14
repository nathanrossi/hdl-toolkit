using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NConsole;
using System.IO;
using HDLToolkit.Xilinx.Implementation.BlockRAM;

namespace HDLToolkit.Console.Commands.Xilinx
{
	[Command("xilinxbmmgen")]
	public class BlockMemoryMapGenerateCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string Output { get; set; }

		[Argument(Position = 0)]
		public string Design { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (string.IsNullOrEmpty(Output) || !Directory.Exists(Output))
			{
				Logger.Instance.WriteError("Output Path '{0}' does not exist", Output);
				return;
			}

			string design = PathHelper.GetFullPath(Design);
			Logger.Instance.WriteVerbose("Selected NCD '{0}'", Design);

			OutputPath location = new OutputPath();
			location.OutputDirectory = PathHelper.GetFullPath(Output);
			location.TemporaryDirectory = SystemHelper.GetTemporaryDirectory();
			location.WorkingDirectory = Environment.CurrentDirectory;
			location.LogDirectory = location.OutputDirectory;

			Logger.Instance.WriteVerbose("Starting Generation");
			BlockMemoryMapGenerator bmmGenerator = new BlockMemoryMapGenerator(location);
			bmmGenerator.NCDFile = design;
			if (bmmGenerator.Build())
			{
				Logger.Instance.WriteInfo("Generation Complete");
			}
			else
			{
				Logger.Instance.WriteError("Generation Failed");
			}

			Logger.Instance.WriteVerbose("Cleaning temporary directory");
			Directory.Delete(location.TemporaryDirectory, true);
		}
	}
}
