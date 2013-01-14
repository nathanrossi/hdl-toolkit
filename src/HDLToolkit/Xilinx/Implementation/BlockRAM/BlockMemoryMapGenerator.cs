using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Xilinx.Parsers;
using System.IO;
using System.Text.RegularExpressions;

namespace HDLToolkit.Xilinx.Implementation.BlockRAM
{
	public class BlockMemoryMapGenerator
	{
		public OutputPath OutputLocation { get; private set; }

		public string NCDFile { get; set; }

		public BlockMemoryMapGenerator(OutputPath output)
		{
			OutputLocation = output;
		}

		public class BlockRAMDescription
		{
			public string NetName { get; set; }
			public string Type { get; set; }
			public string Location { get; set; }
		}

		/// <summary>
		/// Scan a XDL file for Block RAM instances, provide details for the location and type constraints.
		/// </summary>
		/// <param name="xdlFile">XDL file contents</param>
		private static Regex ramblock = new Regex(@"inst.*?""(?<net>.*?)"".*?(?<type>RAMB8|RAM16)_(?<location>\w\d*\w\d*)", RegexOptions.Singleline);
		private static IEnumerable<BlockRAMDescription> FindAllBlockRAMComponents(string xdlFilePath)
		{
			/*
			 * The Xilinx Design Language (XDL) is undocumented this code it based on discovery via experimentation.
			*/
			/*
				inst "if_rom/Mram_test_rom" "RAMB8BWER",placed BRAMSITE2_X3Y40 RAMB8_X0Y21  ,
					cfg ...;
			*/
			if (string.IsNullOrEmpty(xdlFilePath) || !File.Exists(xdlFilePath))
			{
				return null;
			}

			Logger.Instance.WriteDebug("Scanning XDL contents for Block RAMs");

			List<BlockRAMDescription> rams = new List<BlockRAMDescription>();
			int instances = 0;

			// Read the contents line by line, filter 'inst' first then regex match
			using (StreamReader reader = new StreamReader(xdlFilePath))
			{
				string line = null;
				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("inst", StringComparison.InvariantCultureIgnoreCase))
					{
						instances++;
						Match m = ramblock.Match(line);
						if (m.Success)
						{
							BlockRAMDescription description = new BlockRAMDescription();
							description.NetName = m.Groups["net"].Value;
							description.Type = m.Groups["type"].Value;
							description.Location = m.Groups["location"].Value;
							rams.Add(description);

							Logger.Instance.WriteDebug("\tBlock RAM instance '{0}', type {1} at '{2}'",
									description.NetName, description.Type, description.Location);
						}
					}
				}
			}

			Logger.Instance.WriteDebug("Found {0} instance(s)", instances);
			Logger.Instance.WriteDebug("Found {0} block ram(s)", rams.Count);

			return rams;
		}

		public bool Build()
		{
			string projectName = Path.GetFileNameWithoutExtension(NCDFile);

			string projectXdlFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.xdl", projectName));
			string projectBmmFilePath = PathHelper.Combine(OutputLocation.TemporaryDirectory, string.Format("{0}.bmm", projectName));

			// Check NCD file exists
			if (string.IsNullOrEmpty(NCDFile) || !File.Exists(NCDFile))
			{
				throw new FileNotFoundException("NCD File does not exist.");
			}

			// Convert the NCD to xdl
			if (!TranslateNCDFile(NCDFile, projectXdlFilePath))
			{
				throw new Exception("NCD to XDL translation failed");
			}
			
			// scan xdl for blockram instances
			IEnumerable<BlockRAMDescription> blockrams = FindAllBlockRAMComponents(projectXdlFilePath);

			// Write a bmm file
			string bmmContents = TranslateBMM(blockrams);
			File.WriteAllText(projectBmmFilePath, bmmContents);

			// Copy results to output
			OutputLocation.CopyOutputFile(projectBmmFilePath);

			return true;
		}

		private bool TranslateNCDFile(string ncdFile, string xdlFile)
		{
			// Setup Arguments
			List<string> arguments = new List<string>();

			// Default configuration
			arguments.Add("-ncd2xdl"); // NCD to XDL

			// The source NCD
			arguments.Add(string.Format("\"{0}\"", ncdFile));

			// The output XDL
			arguments.Add(string.Format("\"{0}\"", xdlFile));

			// Prepare Process
			XilinxProcess process = new XilinxProcess("xdl", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			StringProcessListener stdout = new StringProcessListener();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());

			process.Listeners.Add(parser);
			process.Listeners.Add(stdout);
			process.WorkingDirectory = OutputLocation.TemporaryDirectory;

			process.Start();
			process.WaitForExit();

			Logger.Instance.WriteDebug(stdout.Output);

			// Check if the process completed correctly
			if (process.CurrentProcess.ExitCode != 0 || !File.Exists(xdlFile))
			{
				return false;
			}
			return true;
		}

		private string TranslateBMM(IEnumerable<BlockRAMDescription> blockrams)
		{
			/*
			 * ADDRESS_SPACE bram_{count} {type} [0x00000000:0x{size}] -- 0 = count, 1 = type, 2 = size
			 *     BUS_BLOCK
			 *         {netname} [{bitwidth}] PLACED = {location};
			 *     END_BUS_BLOCK;
			 * END_ADDRESS_SPACE;
			 */

			StringBuilder bmmContents = new StringBuilder();
			int currentIndex = 0;
			foreach (BlockRAMDescription blockram in blockrams)
			{
				int size = BlockRAMGetSize(blockram.Type);

				// Component
				bmmContents.AppendFormat("ADDRESS_SPACE bram_{0} {1} [0x{2:x8}:0x{3:x8}]", currentIndex, blockram.Type, 0, (size / 8) - 1);
				bmmContents.AppendLine();

				// Block
				bmmContents.AppendLine("\tBUS_BLOCK");
				
				// Bus Line
				bmmContents.AppendFormat("\t\t{0} [7:0] PLACED = {1};", blockram.NetName, blockram.Location);
				bmmContents.AppendLine();

				// Close
				bmmContents.AppendLine("\tEND_BUS_BLOCK;");
				bmmContents.AppendLine("END_ADDRESS_SPACE;");

				currentIndex++;
			}

			return bmmContents.ToString();
		}

		private int BlockRAMGetSize(string type)
		{
			if (string.Compare(type, "RAMB8", true) == 0)
			{
				return 8 * 1024;
			}
			else if (string.Compare(type, "RAMB16", true) == 0)
			{
				return 16 * 1024;
			}
			else if (string.Compare(type, "RAMB18", true) == 0)
			{
				return 16 * 1024;
			}
			else if (string.Compare(type, "RAMB32", true) == 0)
			{
				return 32 * 1024;
			}
			else if (string.Compare(type, "RAMB36", true) == 0)
			{
				return 36 * 1024;
			}
			throw new Exception(string.Format("Unsupported Block RAM device '{0}'", type));
		}
	}
}
