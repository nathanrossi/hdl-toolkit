using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Implementation;
using HDLToolkit.Framework.Devices;
using System.IO;
using HDLToolkit.Xilinx.Implementation.FPGA;

namespace HDLToolkit.Xilinx.Implementation
{
	public class FPGAImplementorInstance : IImplementorInstance
	{
		IImplementor IImplementorInstance.Implementor { get { return Implementor; } }
		public FPGAImplementor Implementor { get; private set; }
		public OutputPath OutputLocation { get; private set; }
		public IImplementationConfiguration Configuration { get; private set; }

		public FPGAImplementorInstance(FPGAImplementor implementor, OutputPath output, IImplementationConfiguration config)
		{
			Implementor = implementor;
			OutputLocation = output;
			Configuration = config;
		}

		public bool Build()
		{
			string netlist = Configuration.NetList;
			string ngdFile = netlist;
			string ncdFile = PathHelper.Combine(OutputLocation.OutputDirectory,
					string.Format("{0}.ncd", Path.GetFileNameWithoutExtension(netlist)));
			string pcfFile = PathHelper.Combine(OutputLocation.OutputDirectory,
					string.Format("{0}.pcf", Path.GetFileNameWithoutExtension(netlist)));

			// Translate if required
			if (string.IsNullOrEmpty(netlist) || !File.Exists(netlist))
			{
				throw new FileNotFoundException("NetList File does not exist.");
			}
			else if (string.Compare(Path.GetExtension(netlist), "ngd", true) != 0)
			{
				// Netlist is not NGD, must be compiled to NGD
				NGDBuilder ngdBuilder = new NGDBuilder(Implementor.Toolchain, OutputLocation);
				ngdBuilder.NetList = netlist;
				ngdBuilder.TargetDevice = Configuration.TargetDevice;
				ngdBuilder.ConstraintsFile = Configuration.Constraints;

				// Translate
				Logger.Instance.WriteVerbose("Running NetList Translation");
				if (!ngdBuilder.Build())
				{
					Logger.Instance.WriteVerbose("Translate Failed");
					return false;
				}
				Logger.Instance.WriteVerbose("Translate Complete");
				ngdFile = PathHelper.Combine(OutputLocation.OutputDirectory,
						string.Format("{0}.ngd", Path.GetFileNameWithoutExtension(netlist)));
			}

			// MAP
			Mapper mapper = new Mapper(Implementor.Toolchain, OutputLocation);
			mapper.NGDFile = ngdFile;
			mapper.TargetDevice = Configuration.TargetDevice;
			Logger.Instance.WriteVerbose("Running Mapping");
			if (!mapper.Build())
			{
				Logger.Instance.WriteVerbose("Mapping Failed");
				return false;
			}
			Logger.Instance.WriteVerbose("Mapping Complete");

			// Place and Route
			PlaceAndRouter placerouter = new PlaceAndRouter(Implementor.Toolchain, OutputLocation);
			placerouter.NCDFile = ncdFile;
			placerouter.PCFFile = pcfFile;
			Logger.Instance.WriteVerbose("Running Place and Route");
			if (!placerouter.Build())
			{
				Logger.Instance.WriteVerbose("Place and Route Failed");
				return false;
			}
			Logger.Instance.WriteVerbose("Place and Route Complete");

			return true;
		}

		public void Dispose()
		{
			// Nothing to dispose of.
		}
	}
}
