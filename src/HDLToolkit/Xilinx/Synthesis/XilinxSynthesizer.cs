// Copyright 2011 Nathan Rossi - http://nathanrossi.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework;
using System.IO;
using HDLToolkit.Xilinx.Parsers;

namespace HDLToolkit.Xilinx.Synthesis
{
	public class XilinxSynthesizer
	{
		public class BuildResult
		{
			public string WorkingDirectory { get; set; }

			public bool Built { get; set; }

			public string BuildLog { get; set; }
		}

		public static BuildResult BuildProject(string workingDirectory, PrjFile projectFile, IModule topModule)
		{
			// Create prj file on disk
			string toplevelComponentName = string.Format("{0}.{1}", topModule.Parent.Name, topModule.Name);
			string projectFilePath = PathHelper.Combine(workingDirectory, "projectfile.prj");
			File.WriteAllText(projectFilePath, projectFile.ToString(ExecutionType.SynthesisOnly));
			string projectXstFilePath = PathHelper.Combine(workingDirectory, "projectfile.xst");
			string projectSyrFilePath = PathHelper.Combine(workingDirectory, "projectfile.syr");
			string projectXstPath = PathHelper.Combine(workingDirectory, "xst");
			string projectTmpPath = PathHelper.Combine(projectXstPath, ".tmp");
			File.WriteAllText(projectXstFilePath, GenerateScript(workingDirectory, projectFilePath, topModule.Name));

			Directory.CreateDirectory(projectXstPath);
			Directory.CreateDirectory(projectTmpPath);

			Logger.Instance.WriteDebug("Top Level component name: {0}", toplevelComponentName);
			Logger.Instance.WriteDebug("Xst path: {0}", projectXstFilePath);

			List<string> arguments = new List<string>();
			arguments.Add(string.Format("-ifn \"{0}\"", projectXstFilePath));
			arguments.Add(string.Format("-ofn \"{0}\"", projectSyrFilePath));

			XilinxProcess process = new XilinxProcess("xst", arguments);
			DefaultMessageParser parser = new DefaultMessageParser();
			StringProcessListener stringParser = new StringProcessListener();
			parser.MessageOccured += ((obj) => obj.WriteToLogger());
			
			process.Listeners.Add(parser);
			process.Listeners.Add(stringParser);
			process.WorkingDirectory = workingDirectory;

			process.Start();
			process.WaitForExit();

			BuildResult buildResult = new BuildResult();
			buildResult.BuildLog = stringParser.Output + "\n\n\n" + stringParser.ErrorOutput;
			buildResult.WorkingDirectory = workingDirectory;

			File.Delete(projectFilePath);
			File.Delete(projectXstFilePath);
			Directory.Delete(PathHelper.Combine(workingDirectory, "xst"), true);

			return buildResult;
		}

		public static string GenerateWorkingDirectory()
		{
			string path = PathHelper.Combine(Path.GetTempPath(), "hdltk-temp", Guid.NewGuid().ToString());
			Logger.Instance.WriteVerbose("Creating temporary working directory at '{0}'", path);
			Directory.CreateDirectory(path);
			return path;
		}

		private static string GenerateScript(string workingDirectory, string projectFilePath, string topModuleName)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("set -tmpdir \"xst/.tmp\"");
			builder.AppendLine("set -xsthdpdir \"xst\"");
			builder.AppendLine("run");
			builder.AppendFormat("-ifn {0}", projectFilePath); // prj file path
			builder.AppendLine();
			builder.AppendLine("-ifmt mixed");
			builder.AppendFormat("-ofn {0}", topModuleName); // output filename
			builder.AppendLine();
			builder.AppendLine("-ofmt NGC"); // output format
			builder.AppendLine("-p xc6slx9-3-csg225"); // device
			builder.AppendFormat("-top {0}", topModuleName); // top level name
			builder.AppendLine();
			builder.AppendLine("-opt_mode Speed");
			builder.AppendLine("-opt_level 1");
			builder.AppendLine("-power NO");
			builder.AppendLine("-iuc NO");
			builder.AppendLine("-keep_hierarchy No");
			builder.AppendLine("-netlist_hierarchy As_Optimized");
			builder.AppendLine("-rtlview Yes");
			builder.AppendLine("-glob_opt AllClockNets");
			builder.AppendLine("-read_cores YES");
			builder.AppendLine("-write_timing_constraints NO");
			builder.AppendLine("-cross_clock_analysis NO");
			builder.AppendLine("-hierarchy_separator /");
			builder.AppendLine("-bus_delimiter <>");
			builder.AppendLine("-case Maintain");
			builder.AppendLine("-slice_utilization_ratio 100");
			builder.AppendLine("-bram_utilization_ratio 100");
			builder.AppendLine("-dsp_utilization_ratio 100");
			builder.AppendLine("-lc Auto");
			builder.AppendLine("-reduce_control_sets Auto");
			builder.AppendLine("-fsm_extract YES -fsm_encoding Auto");
			builder.AppendLine("-safe_implementation No");
			builder.AppendLine("-fsm_style LUT");
			builder.AppendLine("-ram_extract Yes");
			builder.AppendLine("-ram_style Auto");
			builder.AppendLine("-rom_extract Yes");
			builder.AppendLine("-shreg_extract YES");
			builder.AppendLine("-rom_style Auto");
			builder.AppendLine("-auto_bram_packing NO");
			builder.AppendLine("-resource_sharing YES");
			builder.AppendLine("-async_to_sync NO");
			builder.AppendLine("-shreg_min_size 2");
			builder.AppendLine("-use_dsp48 Auto");
			builder.AppendLine("-iobuf YES");
			builder.AppendLine("-max_fanout 100000");
			builder.AppendLine("-bufg 16");
			builder.AppendLine("-register_duplication YES");
			builder.AppendLine("-register_balancing No");
			builder.AppendLine("-optimize_primitives NO");
			builder.AppendLine("-use_clock_enable Auto");
			builder.AppendLine("-use_sync_set Auto");
			builder.AppendLine("-use_sync_reset Auto");
			builder.AppendLine("-iob Auto");
			builder.AppendLine("-equivalent_register_removal YES");
			builder.AppendLine("-slice_utilization_ratio_maxmargin 5");
			return builder.ToString();
		}
	}
}
