using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Synthesis;
using HDLToolkit.Framework;

namespace HDLToolkit.Xilinx.Synthesis
{
	public class XSTSynthesizer : ISynthesizer
	{
		IToolchain ISynthesizer.Toolchain { get { return Toolchain; } }
		public XilinxToolchain Toolchain { get; private set; }

		public XSTSynthesizer(XilinxToolchain toolchain)
		{
			Toolchain = toolchain;
		}

		public ISynthesizerInstance Create(OutputPath output, ISynthesisConfiguration configuration)
		{
			return new XSTInstance(this, output, configuration);
		}
	}
}
