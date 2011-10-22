using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NConsole;
using HDLToolkit.Xilinx;
using HDLToolkit.Framework.Devices;

namespace HDLToolkit.ConsoleCommands
{
	[Command("listknowndevices")]
	public class ListKnownDevicesCommand : BaseCommand
	{
		[Argument(Position = 0)]
		public string SearchTerm { get; set; }

		public override void Execute()
		{
			base.Execute();

			XilinxDeviceTree devTree = new XilinxDeviceTree();
			devTree.LoadDevices();

			if (string.IsNullOrEmpty(SearchTerm))
			{
				foreach (IPartFamily family in devTree.Families)
				{
					DisplayFamilyTree(family);
				}
			}
			else
			{
				IPartFamily family = devTree.FindFamily(SearchTerm);
				if (family == null)
				{
					Logger.Instance.WriteInfo("Valid families:");
					foreach (IPartFamily f in devTree.Families)
					{
						Logger.Instance.WriteInfo("\t{0} - ({1})", f.ShortName, f.Name);
					}

					Logger.Instance.WriteError("Invalid family specified");
				}
				else
				{
					DisplayFamilyTree(family);
				}
			}
		}

		public void DisplayFamilyTree(IPartFamily family)
		{
			Logger.Instance.WriteInfo("o {0} ({1})", family.Name, family.ShortName);
			foreach (IPart part in family.Parts)
			{
				Logger.Instance.WriteInfo("  + {0}", part.Name);
				foreach (IPartDevice device in part.Devices)
				{
					Logger.Instance.WriteInfo("    + {0} ({1})", device.Name, device.Package.Name);

					StringBuilder sb = new StringBuilder();
					foreach (IPartSpeed speed in device.Speeds)
					{
						sb.Append(speed.Name);
						sb.Append(", ");
					}
					sb.Remove(sb.Length - 2, 2);

					Logger.Instance.WriteInfo("      + {0}", sb.ToString());
				}
			}
		}
	}
}
