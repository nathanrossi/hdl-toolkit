using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Implementation;
using HDLToolkit.Framework;

namespace HDLToolkit.Xilinx.Implementation
{
	public class FPGAImplementor : IImplementor
	{
		IToolchain IImplementor.Toolchain { get { return Toolchain; } }
		public XilinxToolchain Toolchain { get; private set; }

		public FPGAImplementor(XilinxToolchain toolchain)
		{
			Toolchain = toolchain;
		}

		public IImplementorInstance Create(OutputPath output, IImplementationConfiguration configuration)
		{
			throw new NotImplementedException();
		}
	}
}
