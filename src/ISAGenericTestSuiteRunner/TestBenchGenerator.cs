using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HDLToolkit;

namespace ISAGenericTestSuiteRunner
{
	public static class TestBenchGenerator
	{
		public static string GenerateTestBench(TestBench test, string workingDirectory, string templateFile)
		{
			string assemblyFile = PathHelper.Combine(workingDirectory, "test.s");
			Logger.Instance.WriteVerbose("Generating Assembly file");
			File.WriteAllText(assemblyFile, test.GenerateAssembly());

			MemoryStream code = GenerateMachineCode(workingDirectory, assemblyFile);

			return TestBenchTemplate(code, templateFile);
		}

		public static MemoryStream GenerateMachineCode(string workingDirectory, string asmFile)
		{
			Logger.Instance.WriteVerbose("Generating Machine code from assembly file using avr-gcc");
			ProcessHelper.ProcessExecutionResult result = ProcessHelper.ExecuteProcess(workingDirectory,
				"avr-gcc", "-x assembler-with-cpp \"" + Path.GetFullPath(asmFile) + "\" -nostartfiles -nodefaultlibs");

			if (!File.Exists(PathHelper.Combine(workingDirectory, "a.out")))
			{
				Logger.Instance.WriteError(result.StandardError);
			}

			Logger.Instance.WriteVerbose("Generating binary output from elf");
			ProcessHelper.ExecuteProcess(workingDirectory, "avr-objcopy", "-O binary a.out a.bin");

			Logger.Instance.WriteVerbose("Reading in binary machine code");
			MemoryStream stream = new MemoryStream();
			using (FileStream reader = new FileStream(PathHelper.Combine(workingDirectory, "a.bin"), FileMode.Open, FileAccess.Read))
			{
				int b = 0;
				while ((b = reader.ReadByte()) != -1)
				{
					stream.WriteByte((byte)b);
				}
			}

			return stream;
		}

		public static string TestBenchTemplate(MemoryStream code, string templateFile)
		{
			StringBuilder data = new StringBuilder();
			string template = File.ReadAllText(templateFile);
			int currentAddress = 0;

			Logger.Instance.WriteVerbose("Generating VHDL Testbench");

			code.Seek(0, SeekOrigin.Begin);

			int currentBlockIndex = 0;
			int currentBlock = 0;
			int currentData = 0;
			while ((currentData = code.ReadByte()) != -1)
			{
				if (currentBlockIndex == 0)
				{
					currentBlockIndex = 1;
					// 
					currentBlock = currentData;
				}
				else
				{
					currentBlockIndex = 0;
					currentBlock |= currentData << 8;
					data.AppendLine(string.Format("\t\t\tipif_addr_data_pair_format(x\"{0:X4}\", x\"{1:X4}\"),", currentAddress, currentBlock));
					currentAddress += 2;
				}
			}

			// terminate array
			data.AppendLine("ipif_addr_data_pair_format(x\"FFFF\", x\"0000\")");

			return template.Replace("#DATAARRAY", data.ToString());
		}
	}
}
