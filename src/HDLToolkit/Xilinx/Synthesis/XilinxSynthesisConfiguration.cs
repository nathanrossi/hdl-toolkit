using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDLToolkit.Xilinx.Synthesis
{
	public class XilinxSynthesisConfiguration
	{
		public OutputPath OutputLocation { get; private set; }

		public string ProjectFilePath { get; set; }
		public string TopModuleName { get; set; }
		public string TargetDevice { get; set; }
		public string OutputFileName { get; set; }

		public XilinxSynthesisConfiguration(OutputPath output)
		{
			OutputLocation = output;
		}

		public string GenerateScript()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("set -tmpdir \"xst/.tmp\"");
			builder.AppendLine("set -xsthdpdir \"xst\"");
			builder.AppendLine("run");
			builder.AppendFormat("-ifn {0}", ProjectFilePath); // prj file path
			builder.AppendLine();
			builder.AppendLine("-ifmt mixed");
			builder.AppendFormat("-ofn {0}", OutputFileName); // output filename
			builder.AppendLine();
			builder.AppendLine("-ofmt NGC"); // output format
			builder.AppendFormat("-p {0}", TargetDevice);
			builder.AppendLine();
			builder.AppendFormat("-top {0}", TopModuleName); // top level name
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
