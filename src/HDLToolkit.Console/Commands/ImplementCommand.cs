using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NConsole;
using System.IO;
using HDLToolkit.Framework.Devices;
using HDLToolkit.Xilinx;
using HDLToolkit.Xilinx.Implementation;
using HDLToolkit.Console.Helpers;
using HDLToolkit.Framework.Implementation;

namespace HDLToolkit.Console.Commands
{
	[Command("implement")]
	public class ImplementCommand : BaseCommand
	{
		[Argument(ShortName = "o", LongName = "output")]
		public string Output { get; set; }

		[Argument(ShortName = "d", LongName = "device")]
		public string Device { get; set; }

		[Argument(ShortName = "c", LongName = "constraints")]
		public string Constraints { get; set; }

		[Argument(Position = 0)]
		public string NetList { get; set; }

		public override void Execute()
		{
			base.Execute();

			if (string.IsNullOrEmpty(Output) || !Directory.Exists(Output))
			{
				Logger.Instance.WriteError("Output Path '{0}' does not exist", Output);
				return;
			}

			string netlist = PathHelper.GetFullPath(NetList);
			string constraints = PathHelper.GetFullPath(Constraints);
			Logger.Instance.WriteVerbose("Selected NetList '{0}'", netlist);
			Logger.Instance.WriteVerbose("Selected Constraints '{0}'", constraints);

			// Search for Part
			DevicePartSpeed device = DeviceHelper.FindDeviceByName(Device);
			if (device == null)
			{
				Logger.Instance.WriteError("Cannot Find Device '{0}'", Device);
				return;
			}
			Logger.Instance.WriteVerbose("Selected device '{0}'", device.Name);

			OutputPath location = new OutputPath();
			location.OutputDirectory = PathHelper.GetFullPath(Output);
			location.TemporaryDirectory = SystemHelper.GetTemporaryDirectory();
			location.WorkingDirectory = Environment.CurrentDirectory;
			location.LogDirectory = location.OutputDirectory;

			Logger.Instance.WriteVerbose("Starting Build");
			GenericImplementationConfiguration config = new GenericImplementationConfiguration();
			config.Constraints = constraints;
			config.NetList = netlist;
			config.TargetDevice = device;

			IImplementor implementor = XilinxHelper.GetCurrentXilinxToolchain().Implementors.FirstOrDefault();
			if (implementor != null)
			{
				using (IImplementorInstance instance = implementor.Create(location, config))
				{
					if (instance.Build())
					{
						Logger.Instance.WriteInfo("Build Complete");
					}
					else
					{
						Logger.Instance.WriteError("Build Failed");
					}
				}
			}

			Logger.Instance.WriteVerbose("Cleaning temporary directory");
			Directory.Delete(location.TemporaryDirectory, true);
		}
	}
}
