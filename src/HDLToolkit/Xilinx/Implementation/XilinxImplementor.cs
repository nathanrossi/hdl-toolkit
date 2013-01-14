using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLToolkit.Framework.Implementation;
using HDLToolkit.Framework.Devices;
using System.IO;

namespace HDLToolkit.Xilinx.Implementation
{
	public class XilinxImplementor : IImplementor
	{
		public string NetList { get; private set; }
		public string ConstraintsFile { get; set; }
		public DevicePartSpeed TargetDevice  { get; private set; }

		public OutputPath OutputLocation  { get; private set; }

		public List<string> Artifacts { get; private set; }
		public Dictionary<string, string> Configuration { get; private set; }

		public XilinxImplementor(OutputPath output, string netlist, DevicePartSpeed device)
		{
			OutputLocation = output;
			NetList = netlist;
			TargetDevice = device;
		}

		public bool Build()
		{
			string ngdFile = NetList;
			string ncdFile = PathHelper.Combine(OutputLocation.OutputDirectory,
					string.Format("{0}.ncd", Path.GetFileNameWithoutExtension(NetList)));
			string pcfFile = PathHelper.Combine(OutputLocation.OutputDirectory,
					string.Format("{0}.pcf", Path.GetFileNameWithoutExtension(NetList)));

			// Translate if required
			if (string.IsNullOrEmpty(NetList) || !File.Exists(NetList))
			{
				throw new FileNotFoundException("NetList File does not exist.");
			}
			else if (string.Compare(Path.GetExtension(NetList), "ngd", true) != 0)
			{
				// Netlist is not NGD, must be compiled to NGD
				XilinxNGDBuilder ngdBuilder = new XilinxNGDBuilder(OutputLocation);
				ngdBuilder.NetList = NetList;
				ngdBuilder.TargetDevice = TargetDevice;
				ngdBuilder.ConstraintsFile = ConstraintsFile;

				// Translate
				Logger.Instance.WriteVerbose("Running NetList Translation");
				if (!ngdBuilder.Build())
				{
					Logger.Instance.WriteVerbose("Translate Failed");
					return false;
				}
				Logger.Instance.WriteVerbose("Translate Complete");
				ngdFile = PathHelper.Combine(OutputLocation.OutputDirectory,
						string.Format("{0}.ngd", Path.GetFileNameWithoutExtension(NetList)));
			}

			// MAP
			XilinxMAP mapper = new XilinxMAP(OutputLocation);
			mapper.NGDFile = ngdFile;
			mapper.TargetDevice = TargetDevice;
			Logger.Instance.WriteVerbose("Running Mapping");
			if (!mapper.Build())
			{
				Logger.Instance.WriteVerbose("Mapping Failed");
				return false;
			}
			Logger.Instance.WriteVerbose("Mapping Complete");

			// Place and Route
			XilinxPAR placerouter = new XilinxPAR(OutputLocation);
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
