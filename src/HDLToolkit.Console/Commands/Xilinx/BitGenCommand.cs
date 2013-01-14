using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NConsole;
using System.IO;
using HDLToolkit.Xilinx.Implementation;

namespace HDLToolkit.Console.Commands.Xilinx
{
	[Command("xilinxbitgen")]
	public class BitGenCommand : BaseCommand
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

			Logger.Instance.WriteVerbose("Starting Build");
			bool successful = false;
			XilinxBitGen synthesizer = new XilinxBitGen(location, design);
			successful = synthesizer.Build();

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
